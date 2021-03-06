/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2020 Ingo Herbote
 * https://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * https://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Core.Model
{
    using System;
    using System.Collections.Generic;
    
    using ServiceStack.OrmLite;

    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;

    /// <summary>
    /// The WatchForum Repository Extensions
    /// </summary>
    public static class WatchForumRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Add a new WatchForum
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="forumId">
        /// The forum id.
        /// </param>
        public static void Add(this IRepository<WatchForum> repository, int userId, int forumId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.Insert(new WatchForum { ForumID = forumId, UserID = userId, Created = DateTime.UtcNow });

            repository.FireNew();
        }

        /// <summary>
        /// Checks if Watch Forum Exists and Returns WatchForum ID
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="forumId">
        /// The forum id.
        /// </param>
        /// <returns>
        /// The <see cref="int?"/>.
        /// </returns>
        public static int? Check(this IRepository<WatchForum> repository, int userId, int forumId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var forum = repository.GetSingle(w => w.UserID == userId && w.ForumID == forumId);

            return forum?.ID;
        }

        /// <summary>
        /// List all Watch Forums by User
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<WatchForum, Forum>> List(this IRepository<WatchForum> repository, int userId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<WatchForum>();

            expression.Join<Forum>((a, b) => b.ID == a.ForumID)
                .Where<WatchForum>((b) => b.UserID == userId)
                .Select();

            return repository.DbAccess.Execute(
                db => db.Connection.SelectMulti<WatchForum, Forum>(expression));
        }

        #endregion
    }
}