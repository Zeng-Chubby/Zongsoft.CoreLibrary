﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Text;

namespace Zongsoft.Diagnostics
{
	public class LogEntry : MarshalByRefObject
	{
		#region 私有变量
		private string _toString;
		#endregion

		#region 成员变量
		private LogLevel _level;
		private object _data;
		private string _source;
		private string _message;
		private string _stackTrace;
		private DateTime _timestamp;
		private Exception _exception;
		#endregion

		#region 构造函数
		public LogEntry(LogLevel level, string source, string message, object data = null)
		{
			_toString = null;
			_stackTrace = string.Empty;
			_source = source == null ? string.Empty : source.Trim();
			_message = message ?? string.Empty;
			_data = data;
			_level = level;
			_timestamp = DateTime.Now;
		}

		public LogEntry(LogLevel level, string source, Exception exception, object data = null)
		{
			_toString = null;
			_stackTrace = string.Empty;
			_source = string.IsNullOrEmpty(source) == null ? (exception == null ? string.Empty : exception.Source) : source.Trim();
			_exception = exception;
			_message = exception == null ? string.Empty : exception.Message;
			_data = data ?? (exception == null ? null : exception.Data);
			_level = level;
			_timestamp = DateTime.Now;
		}
		#endregion

		#region 公共属性
		public LogLevel Level
		{
			get
			{
				return _level;
			}
		}

		public string Source
		{
			get
			{
				return _source;
			}
		}

		public Exception Exception
		{
			get
			{
				return _exception;
			}
		}

		public string Message
		{
			get
			{
				return _message;
			}
		}

		public string StackTrace
		{
			get
			{
				return _stackTrace;
			}
			internal set
			{
				_stackTrace = value;
				_toString = null;
			}
		}

		public object Data
		{
			get
			{
				return _data;
			}
		}

		public DateTime Timestamp
		{
			get
			{
				return _timestamp;
			}
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			return this.ToString(true);
		}

		public string ToString(bool appendStackTrace)
		{
			if(string.IsNullOrEmpty(_toString))
			{
				StringBuilder builder = new StringBuilder(512);

				if(string.IsNullOrEmpty(_source))
					builder.AppendFormat("{0} [{1}]", _level, _timestamp);
				else
					builder.AppendFormat("{0}@{1} [{2}]", _level, _source, _timestamp);

				builder.AppendLine();
				builder.AppendLine(_message);

				if(_data != null)
				{
					builder.AppendLine();
					builder.AppendFormat("Data({0})" + Environment.NewLine, _data.GetType().FullName);

					byte[] bytes = _data as byte[];
					if(bytes == null)
						builder.AppendLine(_data.ToString());
					else
						builder.AppendLine(Zongsoft.Common.Convert.ToHexString(bytes));
				}

				if(appendStackTrace)
				{
					builder.AppendLine();
					builder.AppendLine(_stackTrace);
				}

				_toString = builder.ToString();
			}

			return _toString;
		}
		#endregion
	}
}