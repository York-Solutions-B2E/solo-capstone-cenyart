## Project Backstory – the *why* behind your modernization sprint

> **Context (fictionalized)**  
> For more than a decade, **Apex Health Services** has relied on a home-grown system nick-named **“DOC-Sys”** to generate and track outbound communications—everything from Explanation of Benefits (EOB) PDFs to plastic ID-cards. DOC-Sys grew by accretion: every new print vendor, mail-house, or digital channel bolted on its own status codes (`PRT-C`, `X40`, `IN-BATCH`, etc.). Those codes were **inconsistent across document types and nearly impossible for customer-service reps to interpret**. The result:

* Support teams spent ~20 % of call time deciphering “Where is my document?” questions.  
* Product managers hesitated to add new channels because each one required more one-off status logic.  
* Reporting & analytics teams could not trust the data—“Printed” could mean three different things depending on the feed.

Modernizing the UI alone won’t fix this; **a centralized, authoritative status taxonomy is the foundation**. By normalizing raw vendor codes into a single lifecycle vocabulary (e.g., `ReadyForRelease → Printed → Shipped → Delivered`) the new app delivers:

1. **Clarity for end users**—any document follows the same plain-English stages.  
2. **Easier onboarding of future comm types**—just map them to the master list.  
3. **Cleaner data contracts** for downstream analytics and SLAs.  
4. **Reduced engineering churn**—no more hard-coding edge cases per type.

The legacy pain and the taxonomy strategy echo real modernization guidance in the internal proposal, which stresses the need for “a clear, consistent taxonomy” to replace today’s scattered codes and notes that current lifecycle data is technically available *but buried in dense, technical interfaces that new users find confusing*.

---

## Example lifecycle – **EOB (Explanation of Benefits)**

| # | Event (via RabbitMQ) | New Unified Status | Typical Timestamp |
|---|----------------------|--------------------|-------------------|
| 1 | `EobQueuedForPrint`  | **QueuedForPrinting** | T 0 |
| 2 | `EobPrinted`         | **Printed**            | T 0 + 15 min |
| 3 | `EobInserted`        | **Inserted** *(into envelope)* | T 0 + 1 h |
| 4 | `EobShipped`         | **Shipped**            | T 0 + 6 h |
| 5 | `CarrierScan`        | **InTransit**          | T 0 + 1 day |
| 6 | `EobDelivered`       | **Delivered**          | T 0 + 3 days |
| 7 | *(optional)* `EobReturned` | **Returned**     | varies |

*Behind the scenes:* the Event Simulator in your project will publish these messages to RabbitMQ; the API consumes them, updates `CommunicationStatusHistory`, and SignalR pushes the refreshed status timeline to every open Blazor page.