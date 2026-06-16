using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Complaints.Queries.GetAllComplaints;

public record GetAllComplaintsQuery(int PageNumber): IRequest<PagedResponse<ComplaintResponseModel>>;