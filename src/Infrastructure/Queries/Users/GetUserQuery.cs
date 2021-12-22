using Infrastructure.DTOs.Response;
using MediatR;

namespace Infrastructure.Queries
{
    public class GetUserQuery: IRequest<UserResponse>
    {
        public string Id { get; }

        public GetUserQuery(string id)
        {
            Id = id;
        }
    }
}