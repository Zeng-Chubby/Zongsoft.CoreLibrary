﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Linq;
using System.Collections;

namespace Zongsoft.Data
{
	[Obsolete("Please use ConditionalRange<T> class.")]
	public class ConditionalRange : IConditionalRange
	{
		#region 成员字段
		private object _from;
		private object _to;
		#endregion

		#region 构造函数
		public ConditionalRange()
		{
		}

		public ConditionalRange(object from, object to)
		{
			this.From = from;
			this.To = to;
		}
		#endregion

		#region 公共属性
		public object From
		{
			get
			{
				return _from;
			}
			set
			{
				_from = GetValue(value);
			}
		}

		public object To
		{
			get
			{
				return _to;
			}
			set
			{
				_to = GetValue(value);
			}
		}
		#endregion

		#region 公共方法
		[Zongsoft.Runtime.Serialization.SerializationMember(Behavior = Runtime.Serialization.SerializationMemberBehavior.Ignored)]
		public bool HasValue
		{
			get
			{
				return _from != null || _to != null;
			}
		}

		public Condition ToCondition(string name)
		{
			if(_from == null)
				return _to == null ? null : new Condition(name, _to, ConditionOperator.LessThanEqual);
			else
				return _to == null ? new Condition(name, _from, ConditionOperator.GreaterThanEqual) : new Condition(name, new object[] { _from, _to }, ConditionOperator.Between);
		}
		#endregion

		#region 静态方法
		public static bool IsEmpty(ConditionalRange value)
		{
			return value == null || (value.From == null && value.To == null);
		}

		public static ConditionalRange Parse(string text)
		{
			if(string.IsNullOrWhiteSpace(text))
				return null;

			ConditionalRange result;

			if(TryParse(text, out result))
				return result;

			return null;
		}

		public static bool TryParse(string text, out ConditionalRange result)
		{
			return TryParse<string>(text, out result);
		}

		public static bool TryParse<T>(string text, out ConditionalRange result) where T : IComparable
		{
			result = null;

			if(string.IsNullOrWhiteSpace(text))
				return false;

			text = text.Trim().Trim('(', ')');

			if(string.IsNullOrWhiteSpace(text))
				return false;

			var parts = text.Split('~').Select(p => p.Trim().Trim('?', '*')).ToArray();

			if(!string.IsNullOrWhiteSpace(parts[0]))
			{
				T from;

				if(!Zongsoft.Common.Convert.TryConvertValue(parts[0], out from))
					return false;

				if(result == null)
					result = new ConditionalRange();

				result.From = from;
			}

			if(parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
			{
				T to;

				if(!Zongsoft.Common.Convert.TryConvertValue(parts[1], out to))
					return false;

				if(result == null)
					result = new ConditionalRange();

				result.To = to;
			}

			//最后返回真（即使输出参数为空）
			return true;
		}
		#endregion

		#region 重写方法
		public override string ToString()
		{
			if(_from == null)
			{
				if(_to == null)
					return string.Empty;
				else
					return string.Format("(~{0})", _to.ToString());
			}
			else
			{
				if(_to == null)
					return string.Format("({0})", _from.ToString());
				else
					return string.Format("({0}~{1})", _from.ToString(), _to.ToString());
			}
		}
		#endregion

		#region 私有方法
		private static object GetValue(object value)
		{
			if(value == null || System.Convert.IsDBNull(value))
				return null;

			if(value is string)
			{
				if(string.IsNullOrWhiteSpace((string)value))
					return null;

				return value;
			}

			var items = value as IEnumerable;

			if(items != null)
			{
				var enumerator = items.GetEnumerator();

				if(enumerator != null && enumerator.MoveNext())
				{
					var current = enumerator.Current;
					enumerator.Reset();
					return GetValue(current);
				}

				return null;
			}

			return value;
		}
		#endregion
	}
}
