﻿/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2003-2017 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.ComponentModel;

namespace Zongsoft.Security.Membership
{
	/// <summary>
	/// 表示用户的实体类。
	/// </summary>
	[Serializable]
	public class User : Zongsoft.Common.ModelBase, IMember
	{
		#region 静态字段
		public static readonly string Administrator = "Administrator";
		public static readonly string Guest = "Guest";
		#endregion

		#region 构造函数
		public User()
		{
			this.CreatedTime = DateTime.Now;
		}

		public User(string name, string @namespace) : this(0, name, @namespace)
		{
		}

		public User(uint userId, string name) : this(userId, name, null)
		{
		}

		public User(uint userId, string name, string @namespace)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			this.UserId = userId;
			this.Name = this.FullName = name.Trim();
			this.Namespace = @namespace;
			this.CreatedTime = DateTime.Now;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置成员编号，即用户编号。
		/// </summary>
		uint IMember.MemberId
		{
			get
			{
				return this.UserId;
			}
			set
			{
				this.UserId = value;
			}
		}

		/// <summary>
		/// 获取成员类型，始终返回<see cref="MemberType.User"/>。
		/// </summary>
		MemberType IMember.MemberType
		{
			get
			{
				return MemberType.User;
			}
		}

		/// <summary>
		/// 获取或设置用户编号。
		/// </summary>
		public uint UserId
		{
			get
			{
				return this.GetPropertyValue(() => this.UserId);
			}
			set
			{
				this.SetPropertyValue(() => this.UserId, value);
			}
		}

		/// <summary>
		/// 获取或设置用户名。
		/// </summary>
		public string Name
		{
			get
			{
				return this.GetPropertyValue(() => this.Name);
			}
			set
			{
				//首先处理设置值为空或空字符串的情况
				if(string.IsNullOrWhiteSpace(value))
				{
					//如果当前用户名为空，则说明是通过默认构造函数创建，因此现在不用做处理；否则抛出参数空异常
					if(string.IsNullOrWhiteSpace(this.Name))
						return;

					throw new ArgumentNullException();
				}

				value = value.Trim();

				//用户名的长度必须不少于4个字符
				if(value.Length < 4)
					throw new ArgumentOutOfRangeException($"The '{value}' user name length must be greater than 3.");

				//更新属性内容
				this.SetPropertyValue(() => this.Name, value);
			}
		}

		/// <summary>
		/// 获取或设置用户全称。
		/// </summary>
		public string FullName
		{
			get
			{
				return this.GetPropertyValue(() => this.FullName);
			}
			set
			{
				this.SetPropertyValue(() => this.FullName, value);
			}
		}

		/// <summary>
		/// 获取或设置当前用户所属的命名空间。
		/// </summary>
		public string Namespace
		{
			get
			{
				return this.GetPropertyValue(()=>this.Namespace);
			}
			set
			{
				if(!string.IsNullOrWhiteSpace(value))
				{
					value = value.Trim();

					foreach(var chr in value)
					{
						//命名空间的字符必须是字母、数字、下划线或点号组成
						if(!Char.IsLetterOrDigit(chr) && chr != '_' && chr != '.')
							throw new ArgumentException("The user namespace contains invalid character.");
					}
				}

				//更新属性内容
				this.SetPropertyValue(() => this.Namespace, string.IsNullOrWhiteSpace(value) ? null : value.Trim());
			}
		}

		/// <summary>
		/// 获取或设置对用户的描述文本。
		/// </summary>
		public string Description
		{
			get
			{
				return this.GetPropertyValue(() => this.Description);
			}
			set
			{
				this.SetPropertyValue(() => this.Description, value);
			}
		}

		/// <summary>
		/// 获取或设置用户的头像标识(头像代码或头像图片的URL)。
		/// </summary>
		public string Avatar
		{
			get
			{
				return this.GetPropertyValue(() => this.Avatar);
			}
			set
			{
				if(!string.IsNullOrWhiteSpace(value))
				{
					try
					{
						//获取头像值对应的访问地址
						var url = Zongsoft.IO.FileSystem.GetUrl(value);

						//如果头像访问地址不为空则将它设置为该地址
						if(url != null && url.Length > 0 && !string.Equals(value, url))
							value = url;
					}
					catch { }
				}

				this.SetPropertyValue(() => this.Avatar, value);
			}
		}

		/// <summary>
		/// 获取或设置用户对应的主体对象。
		/// </summary>
		public object Principal
		{
			get
			{
				return this.GetPropertyValue(() => this.Principal);
			}
			set
			{
				this.SetPropertyValue(() => this.Principal, value);
			}
		}

		/// <summary>
		/// 获取或设置用户对应的主体标识。
		/// </summary>
		public string PrincipalId
		{
			get
			{
				return this.GetPropertyValue(() => this.PrincipalId);
			}
			set
			{
				this.SetPropertyValue(() => this.PrincipalId, value);
			}
		}

		/// <summary>
		/// 获取或设置用户的电子邮箱地址，邮箱地址不允许重复。
		/// </summary>
		public string Email
		{
			get
			{
				return this.GetPropertyValue(() => this.Email);
			}
			set
			{
				if(!string.IsNullOrWhiteSpace(value))
				{
					if(!Text.TextRegular.Web.Email.IsMatch(value))
						throw new ArgumentException("Invalid email format.");
				}

				this.SetPropertyValue(() => this.Email, string.IsNullOrWhiteSpace(value) ? null : value.Trim());
			}
		}

		/// <summary>
		/// 获取或设置用户的手机号码。
		/// </summary>
		public string PhoneNumber
		{
			get
			{
				return this.GetPropertyValue(() => this.PhoneNumber);
			}
			set
			{
				this.SetPropertyValue(() => this.PhoneNumber, string.IsNullOrWhiteSpace(value) ? null : value.Trim());
			}
		}

		/// <summary>
		/// 获取或设置用户的状态。
		/// </summary>
		public UserStatus Status
		{
			get
			{
				return this.GetPropertyValue(() => this.Status);
			}
			set
			{
				this.SetPropertyValue(() => this.Status, value);

				//同步设置状态更新时间戳
				this.StatusTimestamp = DateTime.Now;
			}
		}

		/// <summary>
		/// 获取或设置用户状态的更新时间。
		/// </summary>
		public DateTime? StatusTimestamp
		{
			get
			{
				return this.GetPropertyValue(() => this.StatusTimestamp);
			}
			set
			{
				this.SetPropertyValue(() => this.StatusTimestamp, value);
			}
		}

		/// <summary>
		/// 获取或设置当前用户的创建人编号。
		/// </summary>
		public uint? CreatorId
		{
			get
			{
				return this.GetPropertyValue(() => this.CreatorId);
			}
			set
			{
				this.SetPropertyValue(() => this.CreatorId, value);
			}
		}

		/// <summary>
		/// 获取或设置当前用户的创建时间。
		/// </summary>
		public DateTime CreatedTime
		{
			get
			{
				return this.GetPropertyValue(() => this.CreatedTime);
			}
			set
			{
				this.SetPropertyValue(() => this.CreatedTime, value);
			}
		}

		/// <summary>
		/// 获取或设置用户信息的最后修改人编号。
		/// </summary>
		public uint? ModifierId
		{
			get
			{
				return this.GetPropertyValue(() => this.ModifierId);
			}
			set
			{
				this.SetPropertyValue(() => this.ModifierId, value);
			}
		}

		/// <summary>
		/// 获取或设置用户信息的最后修改时间。
		/// </summary>
		public DateTime? ModifiedTime
		{
			get
			{
				return this.GetPropertyValue(() => this.ModifiedTime);
			}
			set
			{
				this.SetPropertyValue(() => this.ModifiedTime, value);
			}
		}
		#endregion

		#region 重写方法
		public override bool Equals(object obj)
		{
			if(obj == null || obj.GetType() != this.GetType())
				return false;

			var other = (User)obj;

			return this.UserId == other.UserId && string.Equals(this.Namespace, other.Namespace, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return (this.Namespace + ":" + this.UserId.ToString()).ToLowerInvariant().GetHashCode();
		}

		public override string ToString()
		{
			if(string.IsNullOrWhiteSpace(this.Namespace))
				return string.Format("[{0}]{1}", this.UserId.ToString(), this.Name);
			else
				return string.Format("[{0}]{1}@{2}", this.UserId.ToString(), this.Name, this.Namespace);
		}
		#endregion

		#region 静态方法
		public static bool IsBuiltin(User user)
		{
			if(user == null)
				return false;

			return IsBuiltin(user.Name);
		}

		public static bool IsBuiltin(string userName)
		{
			return string.Equals(userName, User.Administrator, StringComparison.OrdinalIgnoreCase) ||
			       string.Equals(userName, User.Guest, StringComparison.OrdinalIgnoreCase);
		}
		#endregion
	}
}
