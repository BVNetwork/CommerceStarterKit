/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OxxCommerceStarterKit.Core.Attributes;

namespace OxxCommerceStarterKit.Core.Objects
{
	public class RegisterForm
	{
		[LocalizedDisplayName("/common/account/username")]
		[DataType(DataType.Text)]
		[Required]
		public string UserName { get; set; }

		[LocalizedDisplayName("/common/account/password")]
		[DataType(DataType.Password)]
		[Required]
		public string Password { get; set; }

		[LocalizedDisplayName("/common/account/password")]
		[DataType(DataType.Password)]
		[Required]
		public string PasswordConfirm { get; set; }

		// used by reset password form
		public bool PasswordChanged { get; set; }
		public string Token { get; set; }


		public string ValidationMessage { get; set; }

		public bool MemberClub { get; set; }

		public Dictionary<string, string> AvailableCategories { get; set; }
		public int[] SelectedCategories { get; set; }
	    public bool ConfirmNewsletter { get; set; }
	    public bool ConfirmSms { get; set; }

	    [LocalizedDisplayName("/common/accountpages/firstname_label")]
        public string FirstName { get; set; }

	    [LocalizedDisplayName("/common/accountpages/lastname_label")]
        public string LastName { get; set; }

	    public string Phone { get; set; }
	}
}
