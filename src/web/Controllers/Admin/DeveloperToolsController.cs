/*
Commerce Starter Kit for EPiServer

All rights reserved. See LICENSE.txt in project root.

Copyright (C) 2013-2014 Oxx AS
Copyright (C) 2013-2014 BV Network AS

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Commerce.Marketing;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Forms.Core;
using EPiServer.Forms.Core.Data;
using EPiServer.Forms.Core.Models;
using EPiServer.Forms.Core.Models.Internal;
using EPiServer.Framework.Blobs;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using OxxCommerceStarterKit.Web.Models.Files;

namespace OxxCommerceStarterKit.Web.Controllers.Admin
{

    [System.Web.Mvc.Authorize(Roles = "CmsAdmins")]
    public class DeveloperToolsController : Controller
    {
        private Injected<IFormRepository> _formRepository;
        private Injected<IFormDataRepository> _formDataRepository;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MetaClass()
        {
            return View("MetaClass");
        }

        public ActionResult MetaFields()
        {
            return View("MetaFields");
        }
        public ActionResult Forms()
        {
            FormsViewModel model = new FormsViewModel();
            // No language restrictions
            var formsInfo = _formRepository.Service.GetFormsInfo(null);

            // Get basic information of forms existing in the system.
            // We ONLY find form in the root folder which designed for holding EPiServer forms.
            foreach (var info in formsInfo)
            {
                var friendlyNameInfos = _formRepository.Service.GetFriendlyNameInfos(new FormIdentity(info.FormGuid, null));
                //var dataCount = _formDataRepository.Service.GetSubmissionDataCount(new FormIdentity(info.FormGuid, null),
                //    DateTime.MinValue, DateTime.MaxValue, true);
                var submissionData = _formDataRepository.Service.GetSubmissionData(new FormIdentity(info.FormGuid, null), DateTime.MinValue,
                    DateTime.MaxValue, true);
                model.FormsInfo.Add(new FormInfoModel() { Info = info, NameInfos = friendlyNameInfos, Data = submissionData });

            }

            return View("Forms", model);
        }


        public ActionResult Campaigns(string code)
        {
            var referenceConverter = ServiceLocator.Current.GetInstance<ReferenceConverter>();
            var variantLink = referenceConverter.GetContentLink(code);
            //var repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            //var variant = repo.Get<VariationContent>(variantLink);

            var promotionEngine = ServiceLocator.Current.GetInstance<IPromotionEngine>();
            var currentMarket = ServiceLocator.Current.GetInstance<ICurrentMarket>();
            IEnumerable<DiscountedEntry> entries = promotionEngine.GetDiscountPrices(variantLink, currentMarket.GetCurrentMarket());

            CampaignViewModel campaignModel = new CampaignViewModel();
            campaignModel.DiscountPrices = entries;

            return View("Campaigns", campaignModel);
        }


        [System.Web.Mvc.HttpPost]
        public ActionResult DeleteBlobById()
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string data = new StreamReader(req).ReadToEnd();

            StringBuilder sb = new StringBuilder();
            IBlobFactory blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();

            string[] idList = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string uri in idList)
            {
                Uri blobUri = new Uri(uri);
                sb.AppendFormat("Deleting: {0}", uri);
                DeleteBlob(blobUri, sb, blobFactory);
            }

            ContentResult result = new ContentResult();
            result.Content = sb.ToString();
            return result;
        }

        public ActionResult DeleteBlobs()
        {
            IContentRepository repo = ServiceLocator.Current.GetInstance<IContentRepository>();
            IBlobFactory blobFactory = ServiceLocator.Current.GetInstance<IBlobFactory>();
            var assetHelper = ServiceLocator.Current.GetInstance<ContentAssetHelper>();
            StringBuilder sb = new StringBuilder();

            IEnumerable<ContentReference> contentReferences = null;

            contentReferences = repo.GetDescendents(EPiServer.Core.ContentReference.GlobalBlockFolder);
            DeleteBlobs(contentReferences, repo, sb, blobFactory);
            DeleteContentInAssetFolders(contentReferences, assetHelper, repo, sb, blobFactory);

            contentReferences = repo.GetDescendents(EPiServer.Core.ContentReference.SiteBlockFolder);
            DeleteBlobs(contentReferences, repo, sb, blobFactory);
            DeleteContentInAssetFolders(contentReferences, assetHelper, repo, sb, blobFactory);

            // Private page folders too
            contentReferences = repo.GetDescendents(EPiServer.Core.ContentReference.StartPage);
            DeleteContentInAssetFolders(contentReferences, assetHelper, repo, sb, blobFactory);

            ContentResult result = new ContentResult();
            result.Content = sb.ToString();
            return result;
        }

        private static void DeleteContentInAssetFolders(IEnumerable<ContentReference> contentReferences, ContentAssetHelper assetHelper,
            IContentRepository repo, StringBuilder sb, IBlobFactory blobFactory)
        {
            foreach (ContentReference reference in contentReferences)
            {
                ContentAssetFolder folder = assetHelper.GetAssetFolder(reference);
                if (folder != null && folder.ContentLink != null)
                {
                    var folderContents = repo.GetDescendents(folder.ContentLink);
                    DeleteBlobs(folderContents, repo, sb, blobFactory);
                }
            }
        }

        private static void DeleteBlobs(IEnumerable<ContentReference> contentReferences, IContentRepository repo, StringBuilder sb,
            IBlobFactory blobFactory)
        {
            foreach (ContentReference reference in contentReferences)
            {
                ImageFile file = null;
                try
                {
                    file = repo.Get<ImageFile>(reference);
                }
                catch
                {
                }
                if (file != null)
                {

                    IContentVersionRepository versionRepo = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
                    IEnumerable<ContentVersion> versions = versionRepo.List(file.ContentLink);
                    foreach (ContentVersion version in versions)
                    {
                        var versionOfFile = repo.Get<ImageFile>(version.ContentLink);
                        if (versionOfFile != null)
                        {
                            DeleteBlobInstances(sb, blobFactory, versionOfFile);
                        }
                    }

                    sb.AppendFormat("{0}<br>", file.Name);

                    // Delete old versions
                    DeleteOldVersions(file, sb);
                }
            }
        }

        private static void DeleteBlobInstances(StringBuilder sb, IBlobFactory blobFactory, ImageFile file)
        {
            //DeleteBlob(file.LargeThumbnail, sb, blobFactory);
            //DeleteBlob(file.ListImage, sb, blobFactory);
            //DeleteBlob(file.RelatedProduct, sb, blobFactory);
            //DeleteBlob(file.SimilarProduct, sb, blobFactory);
            //DeleteBlob(file.SliderImage, sb, blobFactory);
            //DeleteBlob(file.box1130, sb, blobFactory);
            //DeleteBlob(file.box370, sb, blobFactory);
            //DeleteBlob(file.box560, sb, blobFactory);
            ////DeleteBlob(file.box750, sb, blobFactory);
            //DeleteBlob(file.width110, sb, blobFactory);
            ////DeleteBlob(file.width1130, sb, blobFactory);
            //DeleteBlob(file.width179, sb, blobFactory);
            //DeleteBlob(file.width279, sb, blobFactory);
            //DeleteBlob(file.width320, sb, blobFactory);
            //DeleteBlob(file.width370, sb, blobFactory);
            //DeleteBlob(file.width379, sb, blobFactory);
            //DeleteBlob(file.width560, sb, blobFactory);
            //DeleteBlob(file.width580, sb, blobFactory);
            ////DeleteBlob(file.width750, sb, blobFactory);
        }

        private static void DeleteOldVersions(ImageFile file, StringBuilder sb)
        {
            IContentVersionRepository versionRepo = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            IEnumerable<ContentVersion> versions = versionRepo.List(file.ContentLink);
            foreach (ContentVersion version in versions)
            {
                if (version.Status != VersionStatus.Published)
                {
                    sb.AppendFormat("Deleting version: {0}", version.ContentLink);

                    versionRepo.Delete(version.ContentLink);

                }
            }

        }

        private static void DeleteBlob(Blob blob, StringBuilder sb, IBlobFactory blobFactory)
        {
            // Deleting 
            if (blob != null)
            {
                DeleteBlob(blob.ID, sb, blobFactory);
            }
        }

        private static void DeleteBlob(Uri blobId, StringBuilder sb, IBlobFactory blobFactory)
        {
            sb.AppendFormat("Deleting: {0}<br>", blobId);
            blobFactory.Delete(blobId);
        }
    }


    public class CampaignViewModel
    {
        public IEnumerable<DiscountedEntry> DiscountPrices { get; set; }
    }

    public class FormsViewModel
    {
        public FormsViewModel()
        {
            FormsInfo = new List<FormInfoModel>();
        }
        public List<FormInfoModel> FormsInfo { get; set; }
    }

    public class FormInfoModel
    {
        public FormInfo Info { get; set; }
        public IEnumerable<Submission> Data { get; set; }
        public IEnumerable<FriendlyNameInfo> NameInfos { get; set; }

        public IEnumerable<string> ExtractEmails()
        {
            List<string> emailFields = new List<string>();
            foreach (var nameInfo in NameInfos)
            {
                string friendlyName = nameInfo.FriendlyName.ToLowerInvariant();
                switch (friendlyName)
                {
                    case "email":
                    case "e-mail":
                        emailFields.Add(nameInfo.ElementId);
                        break;
                }
            }

            List<string> emails = new List<string>();
            foreach (var submission in Data)
            {
                foreach (var field in emailFields)
                {
                    if (submission.Data.ContainsKey(field))
                    {
                        emails.Add(submission.Data[field].ToString());
                    }
                }
            }
            return emails;
        }
    }
}
