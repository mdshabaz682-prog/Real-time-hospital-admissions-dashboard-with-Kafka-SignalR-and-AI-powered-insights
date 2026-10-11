using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HospitalDashboard.Api.Hubs;

[Authorize]
public class AdmissionsHub : Hub
{
}