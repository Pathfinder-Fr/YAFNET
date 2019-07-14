/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2019 Ingo Herbote
 * http://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Pages.Admin
{
    #region Using

    using System;
    using System.Data;
    using System.IO;
    using System.Linq;

    using YAF.Configuration;
   using YAF.Web;
    using YAF.Core;
    using YAF.Core.Model;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;
    using YAF.Utils.Helpers;

    #endregion

    /// <summary>
    /// Class for the Edit Category Page
    /// </summary>
    public partial class editcategory : AdminPage
    {
        #region Methods

        /// <summary>
        /// Handles the Click event of the Cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void CancelClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            YafBuildLink.Redirect(ForumPages.admin_forums);
        }

        /// <summary>
        /// The create images data table.
        /// </summary>
        protected void CreateImagesDataTable()
        {
            using (var dt = new DataTable("Files"))
            {
                dt.Columns.Add("FileID", typeof(long));
                dt.Columns.Add("FileName", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                var dr = dt.NewRow();
                dr["FileID"] = 0;
                dr["FileName"] =
                    $"{YafForumInfo.ForumClientFileRoot}Content/images/spacer.gif"; // use spacer.gif for Description Entry
                dr["Description"] = "None";
                dt.Rows.Add(dr);

                var dir = new DirectoryInfo(
                    this.Request.MapPath($"{YafForumInfo.ForumServerFileRoot}{YafBoardFolders.Current.Categories}"));
                if (dir.Exists)
                {
                    var files = dir.GetFiles("*.*");
                    long fileId = 1;

                    var filesList = from file in files
                                let sExt = file.Extension.ToLower()
                                where sExt == ".png" || sExt == ".gif" || sExt == ".jpg"
                                select file;

                    filesList.ForEach(
                        file =>
                        {
                            dr = dt.NewRow();
                            dr["FileID"] = fileId++;
                            dr["FileName"] =
                                $"{YafForumInfo.ForumClientFileRoot}{YafBoardFolders.Current.Categories}/{file.Name}";
                            dr["Description"] = file.Name;
                            dt.Rows.Add(dr);
                        });
                    
                }

                this.CategoryImages.DataSource = dt;
                this.CategoryImages.DataValueField = "FileName";
                this.CategoryImages.DataTextField = "Description";
                this.CategoryImages.DataBind();
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.IsPostBack)
            {
                return;
            }

            // Populate Category Table
            this.CreateImagesDataTable();

            this.BindData();
        }

        /// <summary>
        /// Creates page links for this page.
        /// </summary>
        protected override void CreatePageLinks()
        {
            this.PageLinks.AddRoot();
            this.PageLinks.AddLink(
                this.GetText("ADMIN_ADMIN", "Administration"),
                YafBuildLink.GetLink(ForumPages.admin_admin));

            this.PageLinks.AddLink(this.GetText("TEAM", "FORUMS"), YafBuildLink.GetLink(ForumPages.admin_forums));
            this.PageLinks.AddLink(this.GetText("ADMIN_EDITCATEGORY", "TITLE"), string.Empty);

            this.Page.Header.Title =
                $"{this.GetText("ADMIN_ADMIN", "Administration")} - {this.GetText("TEAM", "FORUMS")} - {this.GetText("ADMIN_EDITCATEGORY", "TITLE")}";
        }

        /// <summary>
        /// Saves the click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            var categoryId = 0;

            if (this.Request.QueryString.GetFirstOrDefault("c") != null)
            {
                categoryId = int.Parse(this.Request.QueryString.GetFirstOrDefault("c"));
            }

            short sortOrder;
            var name = this.Name.Text.Trim();
            string categoryImage = null;

            if (this.CategoryImages.SelectedIndex > 0)
            {
                categoryImage = this.CategoryImages.SelectedItem.Text;
            }

            if (!ValidationHelper.IsValidPosShort(this.SortOrder.Text.Trim()))
            {
                this.PageContext.AddLoadMessage(
                    this.GetText("ADMIN_EDITCATEGORY", "MSG_POSITIVE_VALUE"),
                    MessageTypes.danger);
                return;
            }

            if (!short.TryParse(this.SortOrder.Text.Trim(), out sortOrder))
            {
                // error...
                this.PageContext.AddLoadMessage(this.GetText("ADMIN_EDITCATEGORY", "MSG_NUMBER"), MessageTypes.danger);
                return;
            }

            if (name.IsNotSet())
            {
                // error...
                this.PageContext.AddLoadMessage(this.GetText("ADMIN_EDITCATEGORY", "MSG_VALUE"), MessageTypes.danger);
                return;
            }

            // save category
            this.GetRepository<Category>().Save(categoryId, name, categoryImage, sortOrder);

            // remove category cache...
            this.Get<IDataCache>().Remove(Constants.Cache.ForumCategory);

            // redirect
            YafBuildLink.Redirect(ForumPages.admin_forums);
        }

        /// <summary>
        /// The bind data.
        /// </summary>
        private void BindData()
        {
            if (this.Request.QueryString.GetFirstOrDefault("c") == null)
            {
                this.LocalizedLabel1.LocalizedTag = this.LocalizedLabel2.LocalizedTag = "NEW_CATEGORY";
                    
                // Currently creating a New Category, and auto fill the Category Sort Order + 1
                var sortOrder = 1;

                try
                {
                    sortOrder = this.GetRepository<Category>().GetHighestSortOrder() + sortOrder;
                }
                catch
                {
                    sortOrder = 1;
                }

                this.SortOrder.Text = sortOrder.ToString();

                return;
            }

            var category = this.GetRepository<Category>().List(this.Request.QueryString.GetFirstOrDefaultAs<int>("c"))
                .FirstOrDefault();

            if (category == null)
            {
                return;
            }

            this.Name.Text = category.Name;
            this.SortOrder.Text = category.SortOrder.ToString();

            this.CategoryNameTitle.Text = this.Label1.Text = this.Name.Text;

            var item = this.CategoryImages.Items.FindByText(category.CategoryImage);

            if (item == null)
            {
                return;
            }

            item.Selected = true;
        }

        #endregion
    }
}