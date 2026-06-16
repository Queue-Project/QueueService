using MediatR;
using QApplication.Responses;

namespace QApplication.UseCases.Complaints.Commands.CreateComplaint;

public record CreateComplaintCommand(int QueueId, string ComplaintText): IRequest<ComplaintResponseModel>;