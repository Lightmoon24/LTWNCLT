using Jilow.Models;
using BoardModel = Jilow.Models.Board;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Jilow.Pages.Board;

public class IndexModel : PageModel
{
    private readonly Supabase.Client _supabase;

    public BoardModel? CurrentBoard { get; set; }

    public List<BoardColumnViewModel> Columns { get; set; } = new();

    public IndexModel(Supabase.Client supabase)
    {
        _supabase = supabase;
    }


    // =========================================================
    // GET /Board
    // =========================================================

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return RedirectToPage("/Account/Login");
        }


        // =====================================================
        // 1. Lấy Board của user
        // =====================================================

        var boardResponse = await _supabase
            .From<BoardModel>()
            .Where(x => x.UserId == userId.Value)
            .Get();

        CurrentBoard =
            boardResponse.Models.FirstOrDefault();


        // =====================================================
        // 2. Nếu chưa có Board -> tạo Board
        // =====================================================

        if (CurrentBoard == null)
        {
            var newBoard = new BoardModel
            {
                UserId = userId.Value,
                Name = "My Board"
            };

            var createBoardResponse =
                await _supabase
                    .From<BoardModel>()
                    .Insert(newBoard);

            CurrentBoard =
                createBoardResponse.Models.FirstOrDefault();

            if (CurrentBoard == null)
            {
                return StatusCode(
                    500,
                    "Không thể tạo Board."
                );
            }


            // Tạo column mặc định
            await CreateDefaultColumns(
                CurrentBoard.Id
            );
        }


        // =====================================================
        // 3. Lấy Columns
        // =====================================================

        var columnResponse = await _supabase
            .From<BoardColumn>()
            .Where(x =>
                x.BoardId == CurrentBoard.Id
            )
            .Get();

        var dbColumns = columnResponse.Models
            .OrderBy(x => x.Position)
            .ToList();


        // =====================================================
        // 4. Nếu Board chưa có column
        // =====================================================

        if (dbColumns.Count == 0)
        {
            await CreateDefaultColumns(
                CurrentBoard.Id
            );


            // Lấy lại columns sau khi tạo
            columnResponse = await _supabase
                .From<BoardColumn>()
                .Where(x =>
                    x.BoardId == CurrentBoard.Id
                )
                .Get();

            dbColumns = columnResponse.Models
                .OrderBy(x => x.Position)
                .ToList();
        }


        // =====================================================
        // 5. Lấy Tickets
        // =====================================================

        foreach (var column in dbColumns)
        {
            var ticketResponse = await _supabase
                .From<Ticket>()
                .Where(x =>
                    x.ColumnId == column.Id
                )
                .Get();

            var tickets = ticketResponse.Models
                .OrderBy(x => x.Position)
                .ToList();


            Columns.Add(
                new BoardColumnViewModel
                {
                    Id = column.Id,

                    BoardId = column.BoardId,

                    Name = column.Name,

                    Position = column.Position,

                    ColorClass =
                        string.IsNullOrWhiteSpace(
                            column.ColorClass)
                            ? "dot-blue"
                            : column.ColorClass,

                    Tickets = tickets
                }
            );
        }


        return Page();
    }


    // =========================================================
    // GET /Board?handler=Ticket&id=123
    // =========================================================

    public async Task<IActionResult> OnGetTicketAsync(
        long id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return new UnauthorizedResult();
        }


        if (id <= 0)
        {
            return NotFound();
        }


        // =====================================================
        // 1. Lấy Board của user
        // =====================================================

        var boardResponse = await _supabase
            .From<BoardModel>()
            .Where(x =>
                x.UserId == userId.Value
            )
            .Get();

        var board =
            boardResponse.Models.FirstOrDefault();


        if (board == null)
        {
            return NotFound();
        }


        // =====================================================
        // 2. Lấy Columns thuộc Board
        // =====================================================

        var columnResponse = await _supabase
            .From<BoardColumn>()
            .Where(x =>
                x.BoardId == board.Id
            )
            .Get();

        var columns =
            columnResponse.Models.ToList();


        var columnIds =
            columns
                .Select(x => x.Id)
                .ToHashSet();


        // =====================================================
        // 3. Lấy Ticket
        // =====================================================

        var ticketResponse = await _supabase
            .From<Ticket>()
            .Where(x =>
                x.Id == id
            )
            .Get();

        var ticket =
            ticketResponse.Models.FirstOrDefault();


        if (ticket == null)
        {
            return NotFound();
        }


        // =====================================================
        // 4. Kiểm tra Ticket có thuộc Board không
        // =====================================================

        if (!columnIds.Contains(ticket.ColumnId))
        {
            return NotFound();
        }


        // =====================================================
        // 5. Tìm Column
        // =====================================================

        var column =
            columns.FirstOrDefault(
                x => x.Id == ticket.ColumnId
            );


        var status =
            column?.Name ?? "Unknown";


        // =====================================================
        // 6. Return JSON
        // =====================================================

        return new JsonResult(
            new
            {
                id = ticket.Id,

                key = ticket.Key,

                title = ticket.Title,

                description =
                    ticket.Description ?? "",

                start =
                    ticket.StartDate?
                        .ToString("dd/MM/yyyy"),

                end =
                    ticket.EndDate?
                        .ToString("dd/MM/yyyy"),

                priority =
                    ticket.Priority ?? "Medium",

                status = status,

                assignee = "Bạn",

                columnId = ticket.ColumnId
            }
        );
    }


    // =========================================================
    // POST /Board?handler=CreateTicket
    // =========================================================

    public async Task<IActionResult> OnPostCreateTicketAsync(
        [FromBody] CreateTicketRequest? request)
    {
        try
        {
            // =================================================
            // 1. Kiểm tra đăng nhập
            // =================================================

            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Phiên đăng nhập đã hết hạn."
                    })
                {
                    StatusCode = 401
                };
            }


            // =================================================
            // 2. Kiểm tra request
            // =================================================

            if (request == null)
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Dữ liệu gửi lên không hợp lệ."
                    })
                {
                    StatusCode = 400
                };
            }


            // =================================================
            // 3. Validate Title
            // =================================================

            if (string.IsNullOrWhiteSpace(
                request.Title))
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Tên công việc không được để trống."
                    })
                {
                    StatusCode = 400
                };
            }


            // =================================================
            // 4. Validate ColumnId
            // =================================================

            if (request.ColumnId <= 0)
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Column không hợp lệ."
                    })
                {
                    StatusCode = 400
                };
            }


            // =================================================
            // 5. Validate ngày
            // =================================================

            if (
                request.StartDate.HasValue &&
                request.EndDate.HasValue &&
                request.EndDate.Value.Date <
                request.StartDate.Value.Date
            )
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Ngày hoàn thành phải sau hoặc bằng ngày bắt đầu."
                    })
                {
                    StatusCode = 400
                };
            }


            // =================================================
            // 6. Lấy Board của user
            // =================================================

            var boardResponse = await _supabase
                .From<BoardModel>()
                .Where(x =>
                    x.UserId == userId.Value
                )
                .Get();

            var board =
                boardResponse.Models.FirstOrDefault();


            if (board == null)
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Không tìm thấy Board của bạn."
                    })
                {
                    StatusCode = 404
                };
            }


            // =================================================
            // 7. Kiểm tra Column thuộc Board
            // =================================================

            var columnResponse = await _supabase
                .From<BoardColumn>()
                .Where(x =>
                    x.Id == request.ColumnId &&
                    x.BoardId == board.Id
                )
                .Get();

            var column =
                columnResponse.Models.FirstOrDefault();


            if (column == null)
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Column không thuộc Board của bạn."
                    })
                {
                    StatusCode = 400
                };
            }


            // =================================================
            // 8. Chuẩn hóa Priority
            // =================================================

            var priority =
                NormalizePriority(
                    request.Priority
                );


            // =================================================
            // 9. Generate Ticket Key
            // =================================================

            var ticketKey =
                await GenerateTicketKey();


            // =================================================
            // 10. Xác định Position
            // =================================================

            var ticketResponse = await _supabase
                .From<Ticket>()
                .Where(x =>
                    x.ColumnId == column.Id
                )
                .Get();

            var existingTickets =
                ticketResponse.Models;


            var nextPosition =
                existingTickets.Any()
                    ? existingTickets.Max(
                        x => x.Position
                    ) + 1
                    : 0;


            // =================================================
            // 11. Tạo Ticket
            // =================================================

            var ticket = new Ticket
            {
                ColumnId =
                    column.Id,

                Key =
                    ticketKey,

                Title =
                    request.Title.Trim(),

                Description =
                    string.IsNullOrWhiteSpace(
                        request.Description)
                        ? null
                        : request.Description.Trim(),

                StartDate =
                    request.StartDate,

                EndDate =
                    request.EndDate,

                Priority =
                    priority,

                Position =
                    nextPosition
            };


            // =================================================
            // 12. INSERT Supabase
            // =================================================

            var insertResponse =
                await _supabase
                    .From<Ticket>()
                    .Insert(ticket);


            var createdTicket =
                insertResponse.Models.FirstOrDefault();


            if (createdTicket == null)
            {
                return new JsonResult(
                    new
                    {
                        success = false,
                        message =
                            "Supabase không trả về Ticket sau khi tạo."
                    })
                {
                    StatusCode = 500
                };
            }


            // =================================================
            // 13. Trả JSON cho Board.js
            // =================================================

            return new JsonResult(
                new
                {
                    success = true,

                    message =
                        "Tạo công việc thành công.",

                    ticket =
                        new
                        {
                            id =
                                createdTicket.Id,

                            key =
                                createdTicket.Key,

                            title =
                                createdTicket.Title,

                            description =
                                createdTicket.Description ?? "",

                            startDate =
                                createdTicket.StartDate?
                                    .ToString("yyyy-MM-dd"),

                            endDate =
                                createdTicket.EndDate?
                                    .ToString("yyyy-MM-dd"),

                            priority =
                                createdTicket.Priority,

                            position =
                                createdTicket.Position,

                            columnId =
                                createdTicket.ColumnId
                        }
                }
            );
        }
        catch (Exception ex)
        {
            return new JsonResult(
                new
                {
                    success = false,

                    message =
                        "Không thể tạo công việc.",

                    error =
                        ex.Message
                })
            {
                StatusCode = 500
            };
        }
    }


    // =========================================================
    // Tạo Column mặc định
    // =========================================================

    private async Task CreateDefaultColumns(
        long boardId)
    {
        var columns =
            new List<BoardColumn>
            {
                new BoardColumn
                {
                    BoardId =
                        boardId,

                    Name =
                        "To do",

                    Position =
                        0,

                    ColorClass =
                        "dot-blue"
                },

                new BoardColumn
                {
                    BoardId =
                        boardId,

                    Name =
                        "Đang làm",

                    Position =
                        1,

                    ColorClass =
                        "dot-yellow"
                },

                new BoardColumn
                {
                    BoardId =
                        boardId,

                    Name =
                        "Review",

                    Position =
                        2,

                    ColorClass =
                        "dot-purple"
                },

                new BoardColumn
                {
                    BoardId =
                        boardId,

                    Name =
                        "Hoàn thành",

                    Position =
                        3,

                    ColorClass =
                        "dot-green"
                }
            };


        await _supabase
            .From<BoardColumn>()
            .Insert(columns);
    }


    // =========================================================
    // Generate WEB-001, WEB-002...
    // =========================================================

    private async Task<string> GenerateTicketKey()
    {
        var response =
            await _supabase
                .From<Ticket>()
                .Get();


        var maxNumber = 0;


        foreach (var ticket in response.Models)
        {
            if (string.IsNullOrWhiteSpace(
                ticket.Key))
            {
                continue;
            }


            var parts =
                ticket.Key.Split(
                    '-',
                    StringSplitOptions.RemoveEmptyEntries
                );


            if (
                parts.Length == 2 &&
                int.TryParse(
                    parts[1],
                    out var number)
            )
            {
                maxNumber =
                    Math.Max(
                        maxNumber,
                        number
                    );
            }
        }


        return $"WEB-{maxNumber + 1:000}";
    }


    // =========================================================
    // Chuẩn hóa Priority
    // =========================================================

    private static string NormalizePriority(
        string? priority)
    {
        if (string.IsNullOrWhiteSpace(priority))
        {
            return "Medium";
        }


        return priority.Trim()
            .ToLowerInvariant() switch
        {
            "low" =>
                "Low",

            "thấp" =>
                "Low",

            "medium" =>
                "Medium",

            "trung bình" =>
                "Medium",

            "high" =>
                "High",

            "cao" =>
                "High",

            _ =>
                "Medium"
        };
    }


    // =========================================================
    // Lấy UserId từ Session
    // =========================================================

    private Guid? GetCurrentUserId()
    {
        var userId =
            HttpContext.Session.GetString(
                "UserId"
            );


        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }


        if (!Guid.TryParse(
            userId,
            out var parsedUserId))
        {
            return null;
        }


        return parsedUserId;
    }
}


// =============================================================
// Request tạo Ticket
// =============================================================

public class CreateTicketRequest
{
    public long ColumnId { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? Priority { get; set; }
        = "Medium";
}


// =============================================================
// ViewModel Column
// =============================================================

public class BoardColumnViewModel
{
    public long Id { get; set; }

    public long BoardId { get; set; }

    public string Name { get; set; }
        = string.Empty;

    public int Position { get; set; }

    public string ColorClass { get; set; }
        = "dot-blue";

    public List<Ticket> Tickets { get; set; }
        = new();
}