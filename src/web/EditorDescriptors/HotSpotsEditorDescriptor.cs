/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.Shell.ObjectEditing.EditorDescriptors;
using OxxCommerceStarterKit.Core;

namespace OxxCommerceStarterKit.Web.EditorDescriptors
{
	[EditorDescriptorRegistration(TargetType = typeof(string), UIHint = Constants.UIHint.HotSpotsEditor)]
	public class HotSpotsEditorDescriptor : EditorDescriptor
	{
		public HotSpotsEditorDescriptor()
		{
			this.ClientEditingClass = "app/editors/HotSpotsEditor";
		}


	}
}
