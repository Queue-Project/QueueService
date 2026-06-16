using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Complaints.Queries.GetComplaintById;

public record GetComplaintByIdQuery(int Id): IRequest<ComplaintResponseModel>;