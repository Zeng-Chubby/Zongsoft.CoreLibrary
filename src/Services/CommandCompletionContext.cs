﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2010-2017 Zongsoft Corporation <http://www.zongsoft.com>
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

namespace Zongsoft.Services
{
	/// <summary>
	/// 表示命令会话执行完成的上下文类。
	/// </summary>
	public class CommandCompletionContext : CommandContext
	{
		#region 成员字段
		private object _result;
		private Exception _exception;
		#endregion

		#region 构造函数
		public CommandCompletionContext(CommandContext context, object result, Exception exception = null) : base(context)
		{
			_result = result;
			_exception = exception;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取当前命令的执行结果。
		/// </summary>
		public object Result
		{
			get
			{
				return _result;
			}
		}

		/// <summary>
		/// 获取命令执行中发生的异常。
		/// </summary>
		public Exception Exception
		{
			get
			{
				return _exception;
			}
			internal set
			{
				_exception = value;
			}
		}
		#endregion
	}
}
