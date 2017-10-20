﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2010-2016 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.CoreLibrary.
 *
 * Zongsoft.CoreLibrary is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.CoreLibrary is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.CoreLibrary; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Zongsoft.Services
{
	public class CommandExecutor : ICommandExecutor
	{
		#region 声明事件
		public event EventHandler<CommandExecutorFailureEventArgs> Failed;
		public event EventHandler<CommandExecutorExecutingEventArgs> Executing;
		public event EventHandler<CommandExecutorExecutedEventArgs> Executed;
		#endregion

		#region 静态字段
		private static CommandExecutor _default;
		#endregion

		#region 成员字段
		private readonly CommandTreeNode _root;
		private ICommandExpressionParser _parser;
		private ICommandOutlet _output;
		private TextWriter _error;
		#endregion

		#region 构造函数
		public CommandExecutor(ICommandExpressionParser parser = null)
		{
			_root = new CommandTreeNode();
			_parser = parser ?? CommandExpressionParser.Instance;
			_output = NullCommandOutlet.Instance;
			_error = CommandErrorWriter.Instance;
		}
		#endregion

		#region 静态属性
		/// <summary>
		/// 获取或设置默认的<see cref="CommandExecutor"/>命令执行器。
		/// </summary>
		public static CommandExecutor Default
		{
			get
			{
				if(_default == null)
					System.Threading.Interlocked.CompareExchange(ref _default, new CommandExecutor(), null);

				return _default;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_default = value;
			}
		}
		#endregion

		#region 公共属性
		public CommandTreeNode Root
		{
			get
			{
				return _root;
			}
		}

		public ICommandExpressionParser Parser
		{
			get
			{
				return _parser;
			}
			set
			{
				if(value == null)
					throw new ArgumentNullException();

				_parser = value;
			}
		}

		public virtual ICommandOutlet Output
		{
			get
			{
				return _output;
			}
			set
			{
				_output = value ?? NullCommandOutlet.Instance;
			}
		}

		public virtual TextWriter Error
		{
			get
			{
				return _error;
			}
			set
			{
				_error = value ?? TextWriter.Null;
			}
		}
		#endregion

		#region 查找方法
		public virtual CommandTreeNode Find(string path)
		{
			return _root.Find(path);
		}
		#endregion

		#region 执行方法
		public object Execute(string commandText, object parameter = null)
		{
			if(string.IsNullOrWhiteSpace(commandText))
				throw new ArgumentNullException(nameof(commandText));

			CommandExecutorContext context = null;

			try
			{
				//创建命令执行器上下文对象
				context = this.CreateExecutorContext(commandText, parameter);

				if(context == null)
					throw new InvalidOperationException("Create executor context failed.");
			}
			catch(Exception ex)
			{
				//激发“Error”事件
				if(!this.OnFailed(context, ex))
					throw;

				return null;
			}

			//创建事件参数对象
			var executingArgs = new CommandExecutorExecutingEventArgs(context);

			//激发“Executing”事件
			this.OnExecuting(executingArgs);

			if(executingArgs.Cancel)
				return executingArgs.Result;

			object result = null;
			bool complateInvoked = false;
			IEnumerable<CommandCompleterDescriptor> descriptors = null;

			try
			{
				//调用执行请求
				result = this.OnExecute(context, out descriptors);
			}
			catch(Exception ex)
			{
				complateInvoked = true;

				//执行命令完成通知
				this.OnCompleted(context, descriptors, ex);

				//激发“Error”事件
				if(!this.OnFailed(context, ex))
					throw;
			}

			//执行命令完成通知
			if(!complateInvoked)
				this.OnCompleted(context, descriptors);

			//创建事件参数对象
			var executedArgs = new CommandExecutorExecutedEventArgs(context, result);

			//激发“Executed”事件
			this.OnExecuted(executedArgs);

			//返回最终的执行结果
			return executedArgs.Result;
		}
		#endregion

		#region 执行实现
		protected virtual object OnExecute(CommandExecutorContext context, out IEnumerable<CommandCompleterDescriptor> descriptors)
		{
			var queue = new Queue<Tuple<CommandExpression, CommandTreeNode>>();
			var expression = context.Expression;

			//设置输出参数默认值
			descriptors = System.Linq.Enumerable.Empty<CommandCompleterDescriptor>();

			while(expression != null)
			{
				//查找指定路径的命令节点
				var node = this.Find(expression.FullPath);

				//如果指定的路径在命令树中是不存在的则抛出异常
				if(node == null)
					throw new CommandNotFoundException(expression.FullPath);

				//将命令表达式的选项集绑定到当前命令上
				if(node.Command != null)
					expression.Options.Bind(node.Command);

				//将找到的命令表达式和对应的节点加入队列中
				queue.Enqueue(new Tuple<CommandExpression, CommandTreeNode>(expression, node));

				//设置下一个待搜索的命令表达式
				expression = expression.Next;
			}

			//如果队列为空则返回空
			if(queue.Count < 1)
				return null;

			//创建输出参数的列表对象
			descriptors = new List<CommandCompleterDescriptor>();

			//初始化第一个输入参数
			var parameter = context.Parameter;

			while(queue.Count > 0)
			{
				var entry = queue.Dequeue();

				//执行队列中的命令
				parameter = this.ExecuteCommand(context, entry.Item1, entry.Item2, parameter);

				//判断命令是否需要清理，如果是则加入到清理列表中
				if(entry.Item2 != null && entry.Item2.Command is ICommandCompletion)
					((IList<CommandCompleterDescriptor>)descriptors).Add(new CommandCompleterDescriptor((ICommandCompletion)entry.Item2.Command, parameter));
			}

			//返回最后一个命令的执行结果
			return parameter;
		}

		protected virtual object ExecuteCommand(CommandExecutorContext context, CommandExpression expression, CommandTreeNode node, object parameter)
		{
			if(context == null)
				throw new ArgumentNullException(nameof(context));

			if(node == null)
				throw new ArgumentNullException(nameof(node));

			if(node.Command == null)
				return null;

			return node.Command.Execute(this.CreateCommandContext(expression, node, parameter));
		}
		#endregion

		#region 保护方法
		protected virtual CommandExecutorContext CreateExecutorContext(string commandText, object parameter)
		{
			//解析当前命令文本
			var expression = this.OnParse(commandText);

			if(expression == null)
				throw new InvalidOperationException($"Invalid command expression text: {commandText}.");

			return new CommandExecutorContext(this, expression, parameter);
		}

		protected virtual CommandContext CreateCommandContext(CommandExpression expression, CommandTreeNode node, object parameter)
		{
			return new CommandContext(this, expression, node, parameter);
		}

		protected virtual CommandExpression OnParse(string text)
		{
			return _parser.Parse(text);
		}
		#endregion

		#region 激发事件
		protected bool OnFailed(CommandExecutorContext context, Exception ex)
		{
			var args = new CommandExecutorFailureEventArgs(context, ex);

			//激发“Failed”事件
			this.OnFailed(args);

			//输出异常信息
			if(!args.ExceptionHandled && args.Exception != null)
				this.Error.WriteLine(args.Exception);

			return args.ExceptionHandled;
		}

		protected virtual void OnFailed(CommandExecutorFailureEventArgs args)
		{
			this.Failed?.Invoke(this, args);
		}

		protected virtual void OnExecuting(CommandExecutorExecutingEventArgs args)
		{
			this.Executing?.Invoke(this, args);
		}

		protected virtual void OnExecuted(CommandExecutorExecutedEventArgs args)
		{
			this.Executed?.Invoke(this, args);
		}
		#endregion

		#region 私有方法
		private void OnCompleted(CommandExecutorContext context, IEnumerable<CommandCompleterDescriptor> descriptors, Exception exception = null)
		{
			if(descriptors == null)
				return;

			foreach(var descriptor in descriptors)
			{
				try
				{
					descriptor.Command.OnCompleted(CommandCompletionContext.Create(context, descriptor.Result, exception));
				}
				catch(Exception ex)
				{
					Zongsoft.Diagnostics.Logger.Error(ex);
				}
			}
		}
		#endregion

		#region 嵌套子类
		protected class CommandCompleterDescriptor
		{
			public ICommandCompletion Command;
			public object Result;

			public CommandCompleterDescriptor(ICommandCompletion command, object result)
			{
				if(command == null)
					throw new ArgumentNullException(nameof(command));

				this.Command = command;
				this.Result = result;
			}
		}

		private class NullCommandOutlet : ICommandOutlet
		{
			#region 单例字段
			public static readonly ICommandOutlet Instance = new NullCommandOutlet();
			#endregion

			public System.Text.Encoding Encoding
			{
				get
				{
					return null;
				}
				set
				{
				}
			}

			public TextWriter Writer
			{
				get
				{
					return TextWriter.Null;
				}
			}

			public void Write(object value)
			{
			}

			public void Write(string text)
			{
			}

			public void Write(CommandOutletColor color, object value)
			{
			}

			public void Write(CommandOutletColor color, string text)
			{
			}

			public void Write(string format, params object[] args)
			{
			}

			public void Write(CommandOutletColor color, string format, params object[] args)
			{
			}

			public void WriteLine()
			{
			}

			public void WriteLine(object value)
			{
			}

			public void WriteLine(string text)
			{
			}

			public void WriteLine(CommandOutletColor color, object value)
			{
			}

			public void WriteLine(CommandOutletColor color, string text)
			{
			}

			public void WriteLine(string format, params object[] args)
			{
			}

			public void WriteLine(CommandOutletColor color, string format, params object[] args)
			{
			}
		}

		private class CommandErrorWriter : TextWriter
		{
			#region 单例字段
			public static readonly TextWriter Instance = new CommandErrorWriter();
			#endregion

			public override Encoding Encoding
			{
				get
				{
					return Encoding.UTF8;
				}
			}

			public override void Write(object value)
			{
				if(value == null)
					return;

				if(value is string)
					Zongsoft.Diagnostics.Logger.Error((string)value);
				else if(value is StringBuilder)
					Zongsoft.Diagnostics.Logger.Error(value.ToString());
				else
					Zongsoft.Diagnostics.Logger.Error("An error occurred.", value);
			}

			public override void Write(string value)
			{
				Zongsoft.Diagnostics.Logger.Error(value);
			}

			public override void Write(string format, params object[] args)
			{
				Zongsoft.Diagnostics.Logger.Error(string.Format(format, args));
			}

			public override Task WriteAsync(string value)
			{
				return Task.Run(() => Zongsoft.Diagnostics.Logger.Error(value));
			}

			public override void WriteLine(object value)
			{
				if(value == null)
					return;

				if(value is string)
					Zongsoft.Diagnostics.Logger.Error((string)value + this.NewLine);
				else if(value is StringBuilder)
					Zongsoft.Diagnostics.Logger.Error(value.ToString() + this.NewLine);
				else
					Zongsoft.Diagnostics.Logger.Error("An error occurred.", value);
			}

			public override void WriteLine(string value)
			{
				Zongsoft.Diagnostics.Logger.Error(value + this.NewLine);
			}

			public override void WriteLine(string format, params object[] args)
			{
				Zongsoft.Diagnostics.Logger.Error(string.Format(format, args) + this.NewLine);
			}

			public override Task WriteLineAsync(string value)
			{
				return Task.Run(() => Zongsoft.Diagnostics.Logger.Error(value + this.NewLine));
			}
		}
		#endregion
	}
}
