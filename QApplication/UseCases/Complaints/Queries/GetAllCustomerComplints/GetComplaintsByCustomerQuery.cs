using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Complaints.Queries.GetAllCustomerComplints;

public record GetComplaintsByCustomerQuery(int PageNumber) : IRequest<PagedResponse<ComplaintResponseModel>>;
