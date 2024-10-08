﻿using GeoTagMap.Models;
using GeoTagMap.Service.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GeoTagMap.RestViewModels;
using GeoTagMap.Service;
using GeoTagMap.WebApi.RestViewModels.Rest;
using GeoTagMap.Models.Common;
using GeoTagMap.WebApi.RestViewModels.View;
using Microsoft.Owin.Security.Provider;
using GeoTagMap.Common.Filtering;
using GeoTagMap.Common.Paging;
using GeoTagMap.Common.Sorting;

namespace GeoTagMap.WebApi.Controllers
{
    [RoutePrefix("api/comment")]
    [Authorize]
    public class CommentController : ApiController
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<HttpResponseMessage> GetAsync(
            int pageNumber = 1,
            int pageSize = 10,
            string orderBy = "Username",
            string sortOrder = "ASC",
            string username = "",
            DateTime? dateCreated = null,
            bool? isReported = null)
        {
            try
            {
                Paging paging = new Paging(pageNumber, pageSize);
                Sorting sort = new Sorting(orderBy, sortOrder);
                CommentFiltering filtering = new CommentFiltering(username, dateCreated, isReported);

                var pagingInfo = await _commentService.GetAllCommentsAsync(paging, sort, filtering);

                if (pagingInfo.List.Count == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }

                return Request.CreateResponse(HttpStatusCode.OK, pagingInfo);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("parentcomments")]
        public async Task<HttpResponseMessage> GetParentCommentsAsync(Guid? eventId = null, Guid? touristSiteId = null, Guid? storyId = null)
        {
            List<CommentView> commentsView = new List<CommentView>();
            try
            {
                var comments = await _commentService.GetAllParentCommentsAsync(eventId, touristSiteId, storyId);
                if (comments.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var comment in comments)
                {
                    commentsView.Add(MapCommentView(comment));
                }

                return Request.CreateResponse(HttpStatusCode.OK, commentsView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("childcomments/{parentId}")]
        public async Task<HttpResponseMessage> GetChildCommentsAsync(Guid parentId)
        {
            List<CommentView> commentsView = new List<CommentView>();
            try
            {
                var comments = await _commentService.GetAllChildCommentsAsync(parentId);
                if (comments.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var comment in comments)
                {
                    commentsView.Add(MapCommentView(comment));
                }

                return Request.CreateResponse(HttpStatusCode.OK, commentsView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("eventcomments/{id}")]
        public async Task<HttpResponseMessage> GetCommentsByEventIdAsync(Guid id)
        {
            List<CommentView> commentsView = new List<CommentView>();
            try
            {
                var comments = await _commentService.GetCommentsByEventIdAsync(id);
                if (comments.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var comment in comments)
                {
                    commentsView.Add(MapCommentView(comment));
                }

                return Request.CreateResponse(HttpStatusCode.OK, commentsView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("touristsitecomments/{id}")]
        public async Task<HttpResponseMessage> GetCommentsByTouristSiteIdAsync(Guid id)
        {
            List<CommentView> commentsView = new List<CommentView>();
            try
            {
                var comments = await _commentService.GetCommentsByTouristSiteIdAsync(id);
                if (comments.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var comment in comments)
                {
                    commentsView.Add(MapCommentView(comment));
                }

                return Request.CreateResponse(HttpStatusCode.OK, commentsView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("storycomments/{id}")]
        public async Task<HttpResponseMessage> GetCommentsByStoryIdAsync(Guid id)
        {
            List<CommentView> commentsView = new List<CommentView>();
            try
            {
                var comments = await _commentService.GetCommentsByStoryIdAsync(id);
                if (comments.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var comment in comments)
                {
                    commentsView.Add(MapCommentView(comment));
                }

                return Request.CreateResponse(HttpStatusCode.OK, commentsView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("usercomments")]
        public async Task<HttpResponseMessage> GetCommentsByUserIdAsync()
        {
            List<CommentView> commentsView = new List<CommentView>();
            try
            {
                var comments = await _commentService.GetCommentsByUserIdAsync();
                if (comments.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var comment in comments)
                {
                    commentsView.Add(MapCommentView(comment));
                }

                return Request.CreateResponse(HttpStatusCode.OK, commentsView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetAsync(Guid id)
        {
            try
            {
                var comment = await _commentService.GetCommentAsync(id);
                if (comment is null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "No user with that Id");
                }
                CommentView commentView = MapCommentView(comment);

                return Request.CreateResponse(HttpStatusCode.OK, commentView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostAsync([FromBody] CommentRest commentRest)
        {
            try
            {
                if (commentRest is null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No data has been entered");
                }
                ICommentModel comment = MapComment(commentRest);
                await _commentService.AddCommentAsync(comment);

                return Request.CreateResponse(HttpStatusCode.Created, "Data has been entered successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPut]
        public async Task<HttpResponseMessage> PutAsync(Guid id, [FromBody] CommentRest commentRest)
        {
            try
            {
                if(commentRest is null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "No data has been entered");
                }
                ICommentModel comment = MapComment(commentRest);
                await _commentService.UpdateCommentAsync(id, comment);

                return Request.CreateResponse(HttpStatusCode.Created, "Data has been updated successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadGateway, ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,")]
        public async Task<HttpResponseMessage> DeleteCommentAsync(Guid id)
        {
            try
            {
                if (await _commentService.GetCommentAsync(id) == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No comment with that id was found");
                }

                await _commentService.DeleteCommentAsync(id);

                return Request.CreateResponse(HttpStatusCode.OK, "Comment has been deleted successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        private ICommentModel MapComment(CommentRest commentRest)
        {
            return new CommentModel
            {
                Text = commentRest.Text,
                StoryId = commentRest.StoryId,
                EventId = commentRest.EventId,
                TouristSiteId = commentRest.TouristSiteId,
                ParentComment = commentRest.ParentComment,
                NumberOfLikes = commentRest.NumberOfLikes,
                IsReported = commentRest.IsReported,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                IsActive = true,
            };
        }

        private CommentView MapCommentView(ICommentModel comment)
        {
            return new CommentView
            {
                Id = comment.Id,
                Text = comment.Text,
                DateCreated = comment.DateCreated,
                DateUpdated = comment.DateUpdated,
                CreatedBy = comment.CreatedBy,
                ParentComment = comment.ParentComment,
                NumberOfLikes = comment.NumberOfLikes,
                IsReported = comment.IsReported,
                User = MapUserView(comment.User),
                Event = MapEventView(comment.Event),
                Story = MapStoryView(comment.Story),
                Site = MapTouristSiteView(comment.Site),
            };
        }

        private EventView MapEventView(IEventModel eventModel)
        {
            if(eventModel == null)
            {
                return null;
            }
            return new EventView
            {
                Id = eventModel.Id,
                Name = eventModel.Name,
                Url = eventModel.Url,
                EventStatus = eventModel.EventStatus,
                Image = eventModel.Image,
                StartDate = eventModel.StartDate,
                EndDate = eventModel.EndDate,
                IsAccessibleForFree = eventModel.IsAccessibleForFree,
                Type = eventModel.Type,

            };
        }
        private StoryView MapStoryView(IStoryModel story)
        {
            if(story == null)
            {
                return null;
            }
            return new StoryView
            {
                Id = story.Id,
                Text = story.Text,
                DateTime = story.DateTime,
            };
        }

        private TouristSiteView MapTouristSiteView(ITouristSitesModel touristSite)
        {
            if(touristSite == null)
            {
                return null;
            }
            return new TouristSiteView
            {
                Id = touristSite.Id,
                Name = touristSite.Name,
                Link = touristSite.Link,
                Popularity = touristSite.Popularity,
                Rating = touristSite.Rating,
                Description = touristSite.Description,
                WebsiteUrl = touristSite.WebsiteUrl,
                Email = touristSite.Email,
                HoursOpen = touristSite.HoursOpen,
                FacebookId = touristSite.FacebookId,
                Instagram = touristSite.Instagram,
                Twitter = touristSite.Twitter,
            };
        }
        private UserModelView MapUserView(IUserModel user)
        {
            return new UserModelView
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Image = user.Image,
                IsActive = user.IsActive,
            };
        }
    }
}