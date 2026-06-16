using MediatR;
using QApplication.Responses;
using QDomain.Enums;

namespace QApplication.UseCases.Complaints.Commands.UpdateComplaintStatus;

public record UpdateComplaintStatusCommand(int Id, ComplaintStatus ComplaintStatus, string? ResponseText): IRequest<ComplaintResponseModel>;