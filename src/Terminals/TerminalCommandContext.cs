﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;

using Zongsoft.Services;

namespace Zongsoft.Terminals
{
	public class TerminalCommandContext : Zongsoft.Services.CommandContext
	{
		#region 构造函数
		public TerminalCommandContext(CommandExecutorContext session, CommandExpression expression, ICommand command, object parameter, IDictionary<string, object> extendedProperties = null) : base(session, expression, command, parameter, extendedProperties)
		{
		}

		public TerminalCommandContext(CommandExecutorContext session, CommandExpression expression, CommandTreeNode commandNode, object parameter, IDictionary<string, object> extendedProperties = null) : base(session, expression, commandNode, parameter, extendedProperties)
		{
		}
		#endregion

		#region 公共属性
		public ITerminal Terminal
		{
			get
			{
				var executor = this.Executor as TerminalCommandExecutor;

				if(executor != null)
					return executor.Terminal;

				return null;
			}
		}
		#endregion
	}
}
