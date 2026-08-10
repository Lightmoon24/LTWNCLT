"use strict";


/* =========================================================
   BOARD CONFIG
   ========================================================= */

const BOARD_URL =
    window.location.pathname;


/* =========================================================
   STATE
   ========================================================= */

let selectedColumnId = null;

let currentTicketId = null;

let isCreatingTicket = false;


/* =========================================================
   DOM READY
   ========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    () => {

        initializeBoard();

    }
);


/* =========================================================
   INITIALIZE
   ========================================================= */

function initializeBoard() {

    console.log(
        "[Board] Initializing..."
    );


    /*
     * Top create button
     */

    const openCreateTask =
        document.getElementById(
            "openCreateTask"
        );


    if (openCreateTask) {

        openCreateTask.addEventListener(
            "click",
            () => {

                const firstColumn =
                    document.querySelector(
                        ".board-column[data-column-id]"
                    );


                if (!firstColumn) {

                    showBoardMessage(
                        "Board chưa có column.",
                        "error"
                    );

                    return;
                }


                const columnId =
                    Number(
                        firstColumn.dataset.columnId
                    );


                openCreateTaskModal(
                    columnId
                );
            }
        );
    }


    /*
     * Inline add task buttons
     */

    document
        .querySelectorAll(
            ".add-task-inline"
        )
        .forEach(
            button => {

                button.addEventListener(
                    "click",
                    () => {

                        const columnId =
                            Number(
                                button.dataset.columnId
                            );


                        console.log(
                            "[Board] Selected column:",
                            columnId
                        );


                        openCreateTaskModal(
                            columnId
                        );
                    }
                );
            }
        );


    /*
     * Create form
     */

    const createForm =
        document.getElementById(
            "createTaskForm"
        );


    if (createForm) {

        createForm.addEventListener(
            "submit",
            handleCreateTask
        );
    }


    /*
     * Close create modal
     */

    const closeCreateModal =
        document.getElementById(
            "closeCreateModal"
        );


    if (closeCreateModal) {

        closeCreateModal.addEventListener(
            "click",
            closeCreateTaskModal
        );
    }


    const cancelCreateTask =
        document.getElementById(
            "cancelCreateTask"
        );


    if (cancelCreateTask) {

        cancelCreateTask.addEventListener(
            "click",
            closeCreateTaskModal
        );
    }

    const nextTaskBtn =
        document.getElementById(
            "nextTaskBtn"
        );


    if (nextTaskBtn) {

        nextTaskBtn.addEventListener(
            "click",
            handleNextTicket
        );
    }

    const deleteTaskBtn =
        document.getElementById(
            "deleteTaskBtn"
        );


    if (deleteTaskBtn) {

        deleteTaskBtn.addEventListener(
            "click",
            handleDeleteTicket
        );
    }


    /*
     * Ticket detail
     */

    document
        .querySelectorAll(
            ".task-card"
        )
        .forEach(
            card => {

                attachTicketClick(
                    card
                );
            }
        );


    /*
     * Close detail modal
     */

    const closeTaskModal =
        document.getElementById(
            "closeTaskModal"
        );


    if (closeTaskModal) {

        closeTaskModal.addEventListener(
            "click",
            closeTaskDetailModal
        );
    }


    /*
     * Close when click outside
     */

    document.addEventListener(
        "click",
        event => {

            const target =
                event.target;


            const createModal =
                document.getElementById(
                    "createTaskModal"
                );


            const taskModal =
                document.getElementById(
                    "taskModal"
                );


            if (
                createModal &&
                target === createModal
            ) {
                closeCreateTaskModal();
            }


            if (
                taskModal &&
                target === taskModal
            ) {
                closeTaskDetailModal();
            }
        }
    );


    /*
     * ESC
     */

    document.addEventListener(
        "keydown",
        event => {

            if (
                event.key !== "Escape"
            ) {
                return;
            }


            closeCreateTaskModal();

            closeTaskDetailModal();
        }
    );


    console.log(
        "[Board] Initialized."
    );
}


/* =========================================================
   CREATE TASK MODAL
   ========================================================= */

function openCreateTaskModal(
    columnId
) {

    if (
        !columnId ||
        Number(columnId) <= 0
    ) {

        showBoardMessage(
            "Column không hợp lệ.",
            "error"
        );

        return;
    }


    selectedColumnId =
        Number(columnId);


    console.log(
        "[Board] Selected column:",
        selectedColumnId
    );


    const modal =
        document.getElementById(
            "createTaskModal"
        );


    if (!modal) {

        console.error(
            "[Board] Không tìm thấy #createTaskModal"
        );

        return;
    }


    const form =
        document.getElementById(
            "createTaskForm"
        );


    if (form) {

        form.reset();
    }


    isCreatingTicket = false;
    modal.setAttribute("aria-hidden", "false");
    modal.classList.add("active");
    modal.style.display = "flex";


    setTimeout(
        () => {

            const titleInput =
                document.getElementById(
                    "taskName"
                );


            if (titleInput) {

                titleInput.focus();
            }

        },
        50
    );
}


/* =========================================================
   CLOSE CREATE MODAL
   ========================================================= */

function closeCreateTaskModal() {

    const modal =
        document.getElementById(
            "createTaskModal"
        );


    if (!modal) {
        return;
    }


    modal.classList.remove(
        "active"
    );
    modal.setAttribute("aria-hidden", "true");
    modal.style.display =
        "none";


    selectedColumnId =
        null;


    isCreatingTicket =
        false;
}


/* =========================================================
   CREATE TASK
   ========================================================= */

async function handleCreateTask(
    event
) {

    event.preventDefault();


    if (isCreatingTicket) {

        return;
    }


    /*
     * Column
     */

    const columnId =
        Number(
            selectedColumnId
        );


    if (
        !columnId ||
        columnId <= 0
    ) {

        showBoardMessage(
            "Chưa xác định được column.",
            "error"
        );

        return;
    }


    /*
     * Inputs
     */

    const titleInput =
        document.getElementById(
            "taskName"
        );


    const descriptionInput =
        document.getElementById(
            "taskDescription"
        );


    const startInput =
        document.getElementById(
            "taskStart"
        );


    const endInput =
        document.getElementById(
            "taskEnd"
        );


    const priorityInput =
        document.getElementById(
            "taskPriority"
        );


    if (!titleInput) {

        showBoardMessage(
            "Không tìm thấy ô tên công việc.",
            "error"
        );

        return;
    }


    const title =
        titleInput.value.trim();


    const description =
        descriptionInput
            ? descriptionInput.value.trim()
            : "";


    const startDate =
        startInput
            ? startInput.value
            : "";


    const endDate =
        endInput
            ? endInput.value
            : "";


    const priority =
        priorityInput
            ? priorityInput.value
            : "Medium";


    /*
     * Validate title
     */

    if (!title) {

        showBoardMessage(
            "Tên công việc không được để trống.",
            "error"
        );

        titleInput.focus();

        return;
    }


    /*
     * Validate date
     */

    if (
        startDate &&
        endDate &&
        endDate < startDate
    ) {

        showBoardMessage(
            "Ngày hoàn thành phải sau hoặc bằng ngày bắt đầu.",
            "error"
        );

        return;
    }


    /*
     * Payload
     */

    const payload = {

        columnId:
            columnId,

        title:
            title,

        description:
            description || null,

        startDate:
            startDate
                ? `${startDate}T00:00:00`
                : null,

        endDate:
            endDate
                ? `${endDate}T00:00:00`
                : null,

        priority:
            priority || "Medium"
    };


    console.log(
        "[Board] CreateTicket payload:",
        payload
    );


    /*
     * Anti Forgery Token
     */

    const form =
        document.getElementById(
            "createTaskForm"
        );


    const antiForgeryToken =
        form
            ?.querySelector(
                'input[name="__RequestVerificationToken"]'
            )
            ?.value;


    console.log(
        "[Board] AntiForgeryToken:",
        antiForgeryToken
            ? "FOUND"
            : "MISSING"
    );


    if (!antiForgeryToken) {

        showBoardMessage(
            "Không tìm thấy Anti-Forgery Token. Hãy tải lại trang.",
            "error"
        );

        return;
    }


    /*
     * Submit button
     */

    const submitButton =
        form?.querySelector(
            'button[type="submit"]'
        );


    const oldButtonHtml =
        submitButton
            ? submitButton.innerHTML
            : "";


    isCreatingTicket =
        true;


    if (submitButton) {

        submitButton.disabled =
            true;

        submitButton.innerHTML =
            '<i class="fa-solid fa-spinner fa-spin"></i> Đang tạo...';
    }


    /*
     * POST
     */

    let response;


    try {

        response =
            await fetch(
                `${BOARD_URL}?handler=CreateTicket`,
                {
                    method: "POST",

                    credentials:
                        "same-origin",

                    headers: {

                        "Content-Type":
                            "application/json",

                        "Accept":
                            "application/json",

                        "RequestVerificationToken":
                            antiForgeryToken
                    },

                    body:
                        JSON.stringify(
                            payload
                        )
                }
            );

    }
    catch (error) {

        console.error(
            "[Board] Network error:",
            error
        );


        showBoardMessage(
            "Không thể kết nối đến máy chủ.",
            "error"
        );


        restoreSubmitButton(
            submitButton,
            oldButtonHtml
        );


        isCreatingTicket =
            false;


        return;
    }


    /*
     * Read response
     */

    const responseText =
        await response.text();


    let data =
        null;


    if (responseText) {

        try {

            data =
                JSON.parse(
                    responseText
                );

        }
        catch (error) {

            console.warn(
                "[Board] Response không phải JSON:",
                responseText
            );
        }
    }


    console.log(
        "[Board] Create response:",
        response.status,
        data
    );


    /*
     * Error
     */

    if (!response.ok) {

        console.error(
            "[Board] Create ticket failed:",
            {
                status:
                    response.status,

                data:
                    data,

                raw:
                    responseText
            }
        );


        let message =
            data?.message ||
            data?.title ||
            responseText ||
            "Không thể tạo công việc.";


        if (
            response.status === 400 &&
            !data?.message
        ) {

            message =
                "Dữ liệu tạo công việc không hợp lệ.";
        }


        if (
            response.status === 401
        ) {

            message =
                data?.message ||
                "Phiên đăng nhập đã hết hạn.";
        }


        if (
            response.status === 403
        ) {

            message =
                "Bạn không có quyền thực hiện thao tác này.";
        }


        showBoardMessage(
            message,
            "error"
        );


        restoreSubmitButton(
            submitButton,
            oldButtonHtml
        );


        isCreatingTicket =
            false;


        return;
    }


    /*
     * Backend success
     */

    if (
        !data ||
        data.success !== true ||
        !data.ticket
    ) {

        console.error(
            "[Board] Response thành công nhưng thiếu ticket:",
            data
        );


        showBoardMessage(
            "Máy chủ không trả về dữ liệu công việc.",
            "error"
        );


        restoreSubmitButton(
            submitButton,
            oldButtonHtml
        );


        isCreatingTicket =
            false;


        return;
    }


    /*
     * Add ticket to UI
     */

    addTicketToBoard(
        data.ticket
    );


    /*
     * Close modal
     */

    closeCreateTaskModal();


    /*
     * Success message
     */

    showBoardMessage(
        data.message ||
            "Tạo công việc thành công.",
        "success"
    );


    /*
     * Restore
     */

    restoreSubmitButton(
        submitButton,
        oldButtonHtml
    );


    isCreatingTicket =
        false;
}


/* =========================================================
   ADD TICKET TO BOARD
   ========================================================= */

function addTicketToBoard(
    ticket
) {

    if (!ticket) {

        return;
    }


    const columnId =
        Number(
            ticket.columnId
        );


    const column =
        document.querySelector(
            `.board-column[data-column-id="${columnId}"]`
        );


    if (!column) {

        console.error(
            "[Board] Không tìm thấy column:",
            columnId
        );

        return;
    }


    const taskList =
        column.querySelector(
            ".task-list"
        );


    if (!taskList) {

        return;
    }


    /*
     * Remove empty message
     */

    const emptyColumn =
        taskList.querySelector(
            ".empty-column"
        );


    if (emptyColumn) {

        emptyColumn.remove();
    }


    /*
     * Create card
     */

    const card =
        createTicketCard(
            ticket
        );


    /*
     * Insert before add button
     */

    const addButton =
        taskList.querySelector(
            ".add-task-inline"
        );


    if (addButton) {

        taskList.insertBefore(
            card,
            addButton
        );

    }
    else {

        taskList.appendChild(
            card
        );
    }


    /*
     * Update count
     */

    updateColumnCount(
        column
    );


    /*
     * Attach click
     */

    attachTicketClick(
        card
    );


    console.log(
        "[Board] Ticket added:",
        ticket
    );
}


/* =========================================================
   CREATE TICKET CARD
   ========================================================= */

function createTicketCard(
    ticket
) {

    const card =
        document.createElement(
            "div"
        );


    card.className =
        "task-card";


    card.dataset.taskId =
        ticket.id;


    card.tabIndex =
        0;


    card.setAttribute(
        "role",
        "button"
    );


    const priority =
        ticket.priority ||
        "Medium";


    const priorityClass =
        getPriorityClass(
            priority
        );


    /*
     * Date
     */

    let dateHtml =
        "";


    if (
        ticket.startDate ||
        ticket.endDate
    ) {

        dateHtml +=
            `
            <div class="task-card-date">
                <i class="fa-regular fa-calendar"></i>
        `;


        if (ticket.startDate) {

            dateHtml +=
                `<span>${formatDate(ticket.startDate)}</span>`;
        }


        if (
            ticket.startDate &&
            ticket.endDate
        ) {

            dateHtml +=
                `<span>-</span>`;
        }


        if (ticket.endDate) {

            dateHtml +=
                `<span>${formatDate(ticket.endDate)}</span>`;
        }


        dateHtml +=
            `</div>`;
    }


    /*
     * Description
     */

    const descriptionHtml =
        ticket.description
            ? `
                <div class="task-card-description">
                    ${escapeHtml(ticket.description)}
                </div>
              `
            : "";


    card.innerHTML =
        `
        <div class="task-card-top">

            <span class="task-key">
                ${escapeHtml(ticket.key || "")}
            </span>

        </div>


        <div class="task-card-title">

            ${escapeHtml(ticket.title || "")}

        </div>


        ${descriptionHtml}


        ${dateHtml}


        <div class="task-card-bottom">

            <span class="task-priority ${priorityClass}">

                ${escapeHtml(priority)}

            </span>


            <div class="task-assignee">

                <span class="assignee-avatar">
                    B
                </span>

                <span class="assignee-name">
                    Bạn
                </span>

            </div>


            <div class="task-comments">

                <i class="fa-regular fa-comment"></i>

                <span>
                    0
                </span>

            </div>

        </div>
        `;


    return card;
}


/* =========================================================
   TICKET CLICK
   ========================================================= */

function attachTicketClick(
    card
) {

    if (
        card.dataset.ticketListener ===
        "true"
    ) {

        return;
    }


    card.dataset.ticketListener =
        "true";


    card.addEventListener(
        "click",
        () => {

            const id =
                Number(
                    card.dataset.taskId
                );


            if (!id) {

                return;
            }


            openTicketDetail(
                id
            );
        }
    );


    card.addEventListener(
        "keydown",
        event => {

            if (
                event.key !== "Enter" &&
                event.key !== " "
            ) {

                return;
            }


            event.preventDefault();


            const id =
                Number(
                    card.dataset.taskId
                );


            if (!id) {

                return;
            }


            openTicketDetail(
                id
            );
        }
    );
}


/* =========================================================
   OPEN TICKET DETAIL
   ========================================================= */

async function openTicketDetail(
    id
) {

    if (!id) {

        return;
    }


    console.log(
        "[Board] Loading ticket:",
        id
    );


    try {

        const response =
            await fetch(
                `${BOARD_URL}?handler=Ticket&id=${encodeURIComponent(id)}`,
                {
                    method: "GET",

                    credentials:
                        "same-origin",

                    headers: {
                        "Accept":
                            "application/json"
                    }
                }
            );


        const responseText =
            await response.text();


        let data =
            null;


        if (responseText) {

            try {

                data =
                    JSON.parse(
                        responseText
                    );

            }
            catch (error) {

                console.warn(
                    "[Board] Ticket response không phải JSON:",
                    responseText
                );
            }
        }


        if (!response.ok) {

            console.error(
                "[Board] Ticket detail failed:",
                response.status,
                data,
                responseText
            );


            showBoardMessage(
                data?.message ||
                    "Không thể tải công việc.",
                "error"
            );

            return;
        }


        populateTicketDetail(
            data
        );


        openTaskDetailModal();

    }
    catch (error) {

        console.error(
            "[Board] Ticket detail error:",
            error
        );


        showBoardMessage(
            "Không thể tải thông tin công việc.",
            "error"
        );
    }
}


/* =========================================================
   POPULATE DETAIL MODAL
   ========================================================= */

function populateTicketDetail(
    ticket
) {

    currentTicketId =
        Number(ticket?.id) || null;

    const nextButton =
        document.getElementById(
            "nextTaskBtn"
        );

    if (nextButton) {
        const currentColumnId =
            Number(ticket?.columnId) || null;

        const currentColumnIndex =
            Array.from(
                document.querySelectorAll(
                    ".board-column[data-column-id]"
                )
            ).findIndex(
                column =>
                    Number(column.dataset.columnId) === currentColumnId
            );

        const hasNextColumn =
            currentColumnIndex >= 0 &&
            currentColumnIndex <
                document.querySelectorAll(
                    ".board-column[data-column-id]"
                ).length - 1;

        nextButton.hidden = !hasNextColumn;
    }

    setText(
        "detailTaskKey",
        ticket.key || ""
    );


    setText(
        "taskModalTitle",
        ticket.title || "Chi tiết công việc"
    );


    setText(
        "detailDescription",
        ticket.description ||
            "Chưa có mô tả."
    );


    setText(
        "detailStartDate",
        ticket.start ||
            "-"
    );


    setText(
        "detailEndDate",
        ticket.end ||
            "-"
    );


    setText(
        "detailPriority",
        ticket.priority ||
            "-"
    );


    setText(
        "detailAssignee",
        ticket.assignee ||
            "Bạn"
    );


    setText(
        "detailStatus",
        ticket.status ||
            "-"
    );
}


/* =========================================================
   OPEN DETAIL MODAL
   ========================================================= */

function openTaskDetailModal() {

    const modal =
        document.getElementById(
            "taskModal"
        );


    if (!modal) {

        console.warn(
            "[Board] Không tìm thấy #taskModal"
        );

        return;
    }


    modal.setAttribute("aria-hidden", "false");
    modal.classList.add(
        "active"
    );


    modal.style.display =
        "flex";
}


/* =========================================================
   CLOSE DETAIL MODAL
   ========================================================= */

function closeTaskDetailModal() {

    const modal =
        document.getElementById(
            "taskModal"
        );


    if (!modal) {

        return;
    }


    modal.classList.remove(
        "active"
    );
    modal.setAttribute("aria-hidden", "true");

    modal.style.display =
        "none";

    currentTicketId = null;
}


/* =========================================================
   UPDATE COLUMN COUNT
   ========================================================= */

function updateColumnCount(
    column
) {

    const count =
        column.querySelector(
            ".task-count"
        );


    if (!count) {

        return;
    }


    const tickets =
        column.querySelectorAll(
            ".task-card"
        );


    count.textContent =
        tickets.length;
}


/* =========================================================
   PRIORITY CLASS
   ========================================================= */

function getPriorityClass(
    priority
) {

    const normalized =
        String(priority || "")
            .trim()
            .toLowerCase();


    switch (normalized) {

        case "high":
        case "cao":
            return "priority-high";


        case "low":
        case "thấp":
            return "priority-low";


        case "medium":
        case "trung bình":
        default:
            return "priority-medium";
    }
}


/* =========================================================
   FORMAT DATE
   ========================================================= */

function formatDate(
    value
) {

    if (!value) {

        return "";
    }


    const parts =
        String(value).split(
            "T"
        )[0].split(
            "-"
        );


    if (
        parts.length !== 3
    ) {

        return value;
    }


    return `${parts[2]}/${parts[1]}/${parts[0]}`;
}


/* =========================================================
   RESTORE SUBMIT BUTTON
   ========================================================= */

function restoreSubmitButton(
    button,
    oldHtml
) {

    if (!button) {

        return;
    }


    button.disabled =
        false;


    button.innerHTML =
        oldHtml ||
        '<i class="fa-solid fa-plus"></i> Tạo công việc';
}


/* =========================================================
   SET TEXT
   ========================================================= */

async function handleNextTicket() {

    if (!currentTicketId) {
        showBoardMessage(
            "Không có công việc nào được chọn.",
            "error"
        );
        return;
    }

    const currentColumnId =
        Number(
            document.querySelector(
                `.task-card[data-task-id="${currentTicketId}"]`
            )?.closest(".board-column")?.dataset?.columnId || 0
        );

    if (!currentColumnId) {
        showBoardMessage(
            "Không xác định được giai đoạn hiện tại.",
            "error"
        );
        return;
    }

    const boardColumns =
        Array.from(
            document.querySelectorAll(
                ".board-column[data-column-id]"
            )
        );

    const currentIndex =
        boardColumns.findIndex(
            column =>
                Number(column.dataset.columnId) === currentColumnId
        );

    const nextColumn =
        currentIndex >= 0 &&
        currentIndex < boardColumns.length - 1
            ? boardColumns[currentIndex + 1]
            : null;

    if (!nextColumn) {
        showBoardMessage(
            "Công việc đã ở giai đoạn cuối.",
            "info"
        );
        return;
    }

    const nextColumnId =
        Number(nextColumn.dataset.columnId);

    try {
        const form = document.getElementById("createTaskForm");
        const antiForgeryToken =
            form?.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const response = await fetch(
            `${BOARD_URL}?handler=NextTicket`,
            {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/json",
                    "Accept": "application/json",
                    "RequestVerificationToken": antiForgeryToken || ""
                },
                body: JSON.stringify({
                    id: currentTicketId,
                    columnId: nextColumnId
                })
            }
        );

        const responseText = await response.text();
        let data = null;

        if (responseText) {
            try {
                data = JSON.parse(responseText);
            } catch (error) {
                console.warn("[Board] Next response không phải JSON:", responseText);
            }
        }

        if (!response.ok || !data?.success) {
            throw new Error(data?.message || "Không thể chuyển tiếp công việc.");
        }

        const currentCard =
            document.querySelector(
                `.task-card[data-task-id="${currentTicketId}"]`
            );

        const currentColumn =
            currentCard?.closest(".board-column");

        const nextTaskList =
            nextColumn.querySelector(".task-list");

        if (currentCard && nextTaskList) {
            const emptyColumn =
                nextTaskList.querySelector(".empty-column");

            if (emptyColumn) {
                emptyColumn.remove();
            }

            nextTaskList.insertBefore(
                currentCard,
                nextTaskList.querySelector(".add-task-inline")
            );
        }

        if (currentColumn) {
            updateColumnCount(currentColumn);
        }

        updateColumnCount(nextColumn);
        closeTaskDetailModal();
        showBoardMessage(data.message || "Công việc đã chuyển sang giai đoạn tiếp theo.", "success");
    }
    catch (error) {
        console.error("[Board] Next ticket error:", error);
        showBoardMessage(error.message || "Không thể chuyển tiếp công việc.", "error");
    }
}

async function handleDeleteTicket() {

    if (!currentTicketId) {
        showBoardMessage(
            "Không có công việc nào được chọn.",
            "error"
        );
        return;
    }

    const confirmed = window.confirm(
        "Bạn có chắc muốn xóa công việc này?"
    );

    if (!confirmed) {
        return;
    }

    try {
        const form = document.getElementById("createTaskForm");
        const antiForgeryToken =
            form?.querySelector('input[name="__RequestVerificationToken"]')?.value;

        const response = await fetch(
            `${BOARD_URL}?handler=DeleteTicket&id=${encodeURIComponent(currentTicketId)}`,
            {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Accept": "application/json",
                    "RequestVerificationToken": antiForgeryToken || ""
                }
            }
        );

        const responseText = await response.text();
        let data = null;

        if (responseText) {
            try {
                data = JSON.parse(responseText);
            } catch (error) {
                console.warn("[Board] Delete response không phải JSON:", responseText);
            }
        }

        if (!response.ok || !data?.success) {
            throw new Error(data?.message || "Không thể xóa công việc.");
        }

        const card = document.querySelector(`.task-card[data-task-id="${currentTicketId}"]`);
        const column = card?.closest(".board-column");

        if (card) {
            card.remove();
        }

        if (column) {
            updateColumnCount(column);
        }

        closeTaskDetailModal();
        showBoardMessage(data.message || "Xóa công việc thành công.", "success");
    }
    catch (error) {
        console.error("[Board] Delete ticket error:", error);
        showBoardMessage(error.message || "Không thể xóa công việc.", "error");
    }
}

function setText(
    id,
    value
) {

    const element =
        document.getElementById(
            id
        );


    if (!element) {

        return;
    }


    element.textContent =
        value ?? "";
}


/* =========================================================
   ESCAPE HTML
   ========================================================= */

function escapeHtml(
    value
) {

    return String(value ?? "")
        .replace(
            /&/g,
            "&amp;"
        )
        .replace(
            /</g,
            "&lt;"
        )
        .replace(
            />/g,
            "&gt;"
        )
        .replace(
            /"/g,
            "&quot;"
        )
        .replace(
            /'/g,
            "&#039;"
        );
}


/* =========================================================
   BOARD MESSAGE
   ========================================================= */

function showBoardMessage(
    message,
    type = "info"
) {

    console.log(
        `[Board ${type}]`,
        message
    );


    /*
     * Nếu project đã có hệ thống toast,
     * ưu tiên dùng nó.
     */

    if (
        typeof window.showToast ===
        "function"
    ) {

        window.showToast(
            message,
            type
        );

        return;
    }


    /*
     * Fallback
     */

    const existing =
        document.getElementById(
            "boardMessage"
        );


    if (existing) {

        existing.remove();
    }


    const messageElement =
        document.createElement(
            "div"
        );


    messageElement.id =
        "boardMessage";


    messageElement.className =
        `board-message board-message-${type}`;


    messageElement.textContent =
        message;


    document.body.appendChild(
        messageElement
    );


    setTimeout(
        () => {

            messageElement.classList.add(
                "show"
            );

        },
        10
    );


    setTimeout(
        () => {

            messageElement.classList.remove(
                "show"
            );


            setTimeout(
                () => {

                    messageElement.remove();

                },
                300
            );

        },
        3000
    );
}