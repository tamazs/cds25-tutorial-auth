using Api.Etc;
using DataAccess.Repositories;

namespace Api.Services;

using Entities = DataAccess.Entities;
using Requests = Api.Models.Dtos.Requests;
using Responses = Api.Models.Dtos.Responses;

public interface IBlogService
{
    Responses.PostDetail GetById(long id);
    IEnumerable<Responses.Post> Newest(Requests.PostsQuery query);
    Task<long> CreateComment(long postId, Requests.CommentFormData data);
}

public class BlogService(
    IRepository<Entities.Post> _postRepository,
    IRepository<Entities.User> _userRepository,
    IRepository<Entities.Comment> _commentRepository
) : IBlogService
{
    public Responses.PostDetail GetById(long id)
    {
        var comments = _commentRepository
            .Query()
            .Join(
                _userRepository.Query(),
                comment => comment.AuthorId,
                user => user.Id,
                (comment, user) => new { comment, user }
            )
            .Where(x => x.comment.PostId == id);
        var post = _postRepository
            .Query()
            .Join(
                _userRepository.Query(),
                post => post.AuthorId,
                user => user.Id,
                (post, user) => new { post, user }
            )
            .GroupJoin(
                comments,
                x => x.post.Id,
                comment => comment.comment.PostId,
                (x, comments) =>
                    new
                    {
                        x.post,
                        x.user,
                        comments
                    }
            )
            .Where(post => post.post.Id == id && post.post.PublishedAt != null)
            .Select(post => new Responses.PostDetail(
                post.post.Id,
                post.post.Title,
                post.post.Content,
                new Responses.Author(post.user.Id, post.user.UserName!),
                post.comments.Select(comment => new Responses.CommentForPost(
                    comment.comment.Id,
                    comment.comment.Content,
                    comment.comment.CreatedAt,
                    new Responses.Author(comment.user.Id, comment.user.UserName!)
                )),
                (DateTime)post.post.PublishedAt!,
                post.post.UpdatedAt > post.post.PublishedAt ? post.post.UpdatedAt : null
            ))
            .FirstOrDefault();
        if (post == null)
        {
            throw new NotFoundError(nameof(Responses.PostDetail), new { Id = id });
        }
        return post;
    }

    public IEnumerable<Responses.Post> Newest(Requests.PostsQuery query)
    {
        const int pageSize = 10;
        return _postRepository
            .Query()
            .Where(post => post.PublishedAt != null)
            .OrderByDescending(post => post.PublishedAt)
            .Skip(query.Page * pageSize)
            .Take(pageSize)
            .Join(
                _userRepository.Query(),
                post => post.AuthorId,
                user => user.Id,
                (post, user) => new { post, user }
            )
            .Select(x => new Responses.Post(
                x.post.Id,
                x.post.Title,
                x.post.Content,
                new Responses.Author(x.user.Id, x.user!.UserName!),
                (DateTime)x.post.PublishedAt!,
                x.post.UpdatedAt > x.post.PublishedAt ? x.post.UpdatedAt : null
            ));
    }

    public async Task<long> CreateComment(long postId, Requests.CommentFormData data)
    {
        var comment = new Entities.Comment
        {
            Content = data.Content,
            AuthorId = data.AuthorId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
        };
        await _commentRepository.Add(comment);
        return comment.Id;
    }
}
