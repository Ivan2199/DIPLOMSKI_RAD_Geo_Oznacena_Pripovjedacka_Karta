using GeoTagMap.Service;
using GeoTagMap.Service.Common;
using GeoTagMap.WebApi.RestViewModels.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using GeoTagMap.Models.Common;
using GeoTagMap.WebApi.RestViewModels.Rest;
using GeoTagMap.Models;

namespace GeoTagMap.WebApi.Controllers
{
    [RoutePrefix("api/like")]
    [Authorize]
    public class LikeController : ApiController
    {
        private readonly ILikeService _likeService;
        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<HttpResponseMessage> GetAsync()
        {
            List<LikeView> likesView = new List<LikeView>();
            try
            {
                var likes = await _likeService.GetAllLikesAsync();
                if (likes.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var like in likes)
                {
                    likesView.Add(MapLikeView(like));
                }

                return Request.CreateResponse(HttpStatusCode.OK, likesView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("comment/{commentId}")]
        public async Task<HttpResponseMessage> GetLikeByCommentAndUserIdAsync(Guid commentId)
        {
            try
            {
                var like = await _likeService.GetLikeByCommentAndUserIdAsync(commentId);
                if (like is null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "No user with that Id");
                }
                LikeView likeView = MapLikeView(like);

                return Request.CreateResponse(HttpStatusCode.OK, likeView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("event/{eventId}")]
        public async Task<HttpResponseMessage> GetLikeByEventAndUserIdAsync(Guid eventId)
        {
            try
            {
                var like = await _likeService.GetLikeByEventAndUserIdAsync(eventId);
                if (like is null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "No user with that Id");
                }
                LikeView likeView = MapLikeView(like);

                return Request.CreateResponse(HttpStatusCode.OK, likeView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("touristsite/{touristSiteId}")]
        public async Task<HttpResponseMessage> GetLikeByTouristSiteAndUserIdAsync(Guid touristSiteId)
        {
            try
            {
                var like = await _likeService.GetLikeByTouristSiteAndUserIdAsync(touristSiteId);
                if (like is null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "No user with that Id");
                }
                LikeView likeView = MapLikeView(like);

                return Request.CreateResponse(HttpStatusCode.OK, likeView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("story/{id}")]
        public async Task<HttpResponseMessage> GetLikeByStoryAndUserIdAsync(Guid id)
        {
            try
            {
                var like = await _likeService.GetLikeByStoryAndUserIdAsync(id);
                if (like is null)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "No user with that Id");
                }
                LikeView likeView = MapLikeView(like);

                return Request.CreateResponse(HttpStatusCode.OK, likeView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("eventlikes/{id}")]
        public async Task<HttpResponseMessage> GetLikesByEventIdAsync(Guid id)
        {
            List<LikeView> likesView = new List<LikeView>();
            try
            {
                var likes = await _likeService.GetAllLikesByEventIdAsync(id);
                if (likes.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var like in likes)
                {
                    likesView.Add(MapLikeView(like));
                }

                return Request.CreateResponse(HttpStatusCode.OK, likesView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("commentlikes/{id}")]
        public async Task<HttpResponseMessage> GetLikesByCommentIdAsync(Guid id)
        {
            List<LikeView> likesView = new List<LikeView>();
            try
            {
                var likes = await _likeService.GetAllLikesByCommentIdAsync(id);
                if (likes.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var like in likes)
                {
                    likesView.Add(MapLikeView(like));
                }

                return Request.CreateResponse(HttpStatusCode.OK, likesView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("userlikes")]
        public async Task<HttpResponseMessage> GetLikesByUserIdAsync(string likesOf = "")
        {
            List<LikeView> likesView = new List<LikeView>();
            try
            {
                var likes = await _likeService.GetAllLikesByUserIdAsync(likesOf);
                if (likes.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var like in likes)
                {
                    likesView.Add(MapLikeView(like));
                }

                return Request.CreateResponse(HttpStatusCode.OK, likesView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("touristsitelikes/{id}")]
        public async Task<HttpResponseMessage> GetLikesByTouristSiteIdAsync(Guid id)
        {
            List<LikeView> likesView = new List<LikeView>();
            try
            {
                var likes = await _likeService.GetAllLikesByTouristSiteIdAsync(id);
                if (likes.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var like in likes)
                {
                    likesView.Add(MapLikeView(like));
                }

                return Request.CreateResponse(HttpStatusCode.OK, likesView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("storylikes/{id}")]
        public async Task<HttpResponseMessage> GetLikesByStoryIdAsync(Guid id)
        {
            List<LikeView> likesView = new List<LikeView>();
            try
            {
                var likes = await _likeService.GetAllLikesByStoryIdAsync(id);
                if (likes.Count() == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent, "List is empty");
                }
                foreach (var like in likes)
                {
                    likesView.Add(MapLikeView(like));
                }

                return Request.CreateResponse(HttpStatusCode.OK, likesView);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<HttpResponseMessage> PostAsync([FromBody] LikeRest likeRest)
        {
            try
            {
                if (likeRest is null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "No data has been entered");
                }
                ILikeModel likeModel = MapLike(likeRest);
                await _likeService.AddLikeAsync(likeModel);

                return Request.CreateResponse(HttpStatusCode.Created, "Data has been entered successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [Route("{id}")]
        public async Task<HttpResponseMessage> DeleteAsync(Guid id)
        {
            try
            {
                if (await _likeService.GetLikeAsync(id) == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "No tourist site with that id was found");
                }

                await _likeService.DeleteLikeAsync(id);

                return Request.CreateResponse(HttpStatusCode.OK, "Tourist site has been deleted successfully");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private ILikeModel MapLike(LikeRest likeRest)
        {
            return new LikeModel
            {
                CommentId = likeRest.CommentId,
                EventId = likeRest.EventId,
                TouristSiteId = likeRest.TouristSiteId,
                StoryId = likeRest.StoryId,
                IsLike = likeRest.IsLike,
                IsActive = true,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
            };
        }
        private LikeView MapLikeView(ILikeModel like)
        {
            return new LikeView
            {
                Id = like.Id,
                IsLike = like.IsLike,
                CommentId = like.CommentId,
                EventId = like.EventId,
                TouristSiteId = like.TouristSiteId,
                StoryId = like.StoryId,
                UserId = like.UserId,
                DateCreated = like.DateCreated,
            };
        }
    }
}