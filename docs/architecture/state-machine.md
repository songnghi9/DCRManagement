# DCR Workflow State Machine

## Sơ đồ trạng thái

```
                    ┌─────────┐
                    │  DRAFT  │◄──────────────────────────────────┐
                    └────┬────┘                                   │
                         │ [Submit]                               │
                         │ Actor: Engineer | Admin                │
                         ▼                                        │
               ┌──────────────────┐                              │
               │  PENDING REVIEW  │──[Cancel]──► CANCELLED        │
               └────────┬─────────┘                              │
                        │ [StartReview]                           │
                        │ Actor: Reviewer | Admin                 │
                        │ (must be AssignedReviewer)              │
                        ▼                                         │
               ┌─────────────────┐                               │
               │  UNDER REVIEW   │──[Reject]──► REJECTED ─────────┘
               └────────┬────────┘              (re-submit allowed)
                        │ [SendToApproval]
                        │ Actor: Reviewer | Admin
                        ▼
             ┌───────────────────┐
             │ PENDING APPROVAL  │──[Reject]──► REJECTED
             └─────────┬─────────┘
                       │ [Approve]
                       │ Actor: Approver | Admin
                       │ (must be AssignedApprover)
                       ▼
               ┌──────────────┐
               │   APPROVED   │──[Close]──► CLOSED (terminal)
               └──────────────┘
                  Actor: Admin | Approver
```

## Bảng chuyển trạng thái (VALID_TRANSITIONS)

| Từ trạng thái | Hành động | Sang trạng thái | Role được phép |
|--------------|-----------|-----------------|----------------|
| Draft | Submit | PendingReview | Engineer, Admin |
| Draft | Cancel | Cancelled | Engineer, Admin |
| PendingReview | StartReview | UnderReview | Reviewer*, Admin |
| PendingReview | Reject | Rejected | Reviewer, Approver, Admin |
| PendingReview | Cancel | Cancelled | Engineer, Admin |
| UnderReview | SendToApproval | PendingApproval | Reviewer*, Admin |
| UnderReview | Reject | Rejected | Reviewer, Approver, Admin |
| PendingApproval | Approve | Approved | Approver**, Admin |
| PendingApproval | Reject | Rejected | Approver**, Admin |
| Approved | Close | Closed | Admin, Approver |
| Rejected | Submit | PendingReview | Engineer, Admin |

`*` = phải là `AssignedReviewerId` của DCR đó  
`**` = phải là `AssignedApproverId` của DCR đó

## Terminal States

- **Closed** — hoàn tất, không thể thực hiện thêm action
- **Cancelled** — bị hủy, không thể thực hiện thêm action

## Ghi chú quan trọng

1. **Re-submission sau Reject** — DCR bị Reject có thể được Submit lại (trở về PendingReview). Engineer có thể sửa nội dung trước khi submit lại.

2. **ActualCompletionDate** — tự động set khi DCR chuyển sang `Closed`.

3. **Comment bắt buộc** khi Reject hoặc Cancel (enforce ở UI layer, `DCRDetailPresenter`).

4. **Email notification** — fire-and-forget sau mỗi transition thành công. Lỗi email không block workflow.

## Source Code References

```
WorkflowService.VALID_TRANSITIONS    — HashSet<(DCRStatus, ApprovalAction)>
WorkflowService.ACTION_PERMISSIONS   — Dictionary<ApprovalAction, UserRole[]>
WorkflowService.ACTION_RESULT_STATUS — Dictionary<ApprovalAction, DCRStatus>
WorkflowService.ExecuteActionAsync() — Entry point cho mọi transition
WorkflowService.GetAvailableActions() — Trả về actions hợp lệ cho UI hiển thị
```

## ApprovalAction Enum

```csharp
Submit = 0         // Draft → PendingReview
StartReview = 1    // PendingReview → UnderReview
SendToApproval = 2 // UnderReview → PendingApproval
Approve = 3        // PendingApproval → Approved
Reject = 4         // Any → Rejected
Close = 5          // Approved → Closed
Cancel = 6         // Draft/PendingReview → Cancelled
```

## DCRStatus Enum

```csharp
Draft = 0
PendingReview = 1
UnderReview = 2
PendingApproval = 3
Approved = 4
Rejected = 5
Closed = 6
Cancelled = 7
```
