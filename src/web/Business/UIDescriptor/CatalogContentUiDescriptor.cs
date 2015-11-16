/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Shell;
using OxxCommerceStarterKit.Web.Models.Catalog;

namespace OxxCommerceStarterKit.Web.Business.UIDescriptor
{
    [UIDescriptorRegistration]
    public class CameraVariationDefaultViewUiDescriptor: UIDescriptor<DigitalCameraVariationContent>
    {
        public CameraVariationDefaultViewUiDescriptor()
            : base()
        {
            DefaultView = CmsViewNames.OnPageEditView;
        }
    }

}
