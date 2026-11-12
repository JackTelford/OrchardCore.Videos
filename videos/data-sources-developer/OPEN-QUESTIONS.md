# Open questions: Data Sources: Expose Your Module's Data to Pipelines

The narration states only what was read in the Orchard Core source (commit `92db4a83`) and in
`src/docs/reference/modules/DataSources/README.md` and `DataPipelines/README.md`. Nothing was run. A person must check
the following before or while recording.

## On a running site

1. **The dialog lists the Users data sets.** With the Blog recipe and **Data Pipelines** enabled, signed in as `admin`,
   **Add a data source** opens a dialog whose list on the left has an entry **Users** with the badge **2**, and whose
   cards include **Users** (**One row per user.**) and **User roles** (**One row per role of each user.**). Read from
   `OrchardCore.DataPipelines/DataSources/Users/UsersDataSource.cs:48-52,170-175` and
   `Assets/designer/src/dataSets/DataSetPicker.vue`. The selectors of the plan (`data-set-category-Users`,
   `data-set-Users::Users`) come from the Vue source, not from a rendered page: confirm that they match, in particular
   the colons in `data-set-Users::Users`.
2. **The designer loads.** The designer is a Vue application built from `OrchardCore.DataPipelines/Assets/designer`
   (`Assets.json`). Confirm that the checkout used to record builds it, and that the toolbar shows **Add a data source**.
3. **The step panel opens by itself** after **Add** (`App.vue:296-306`, when the step has an editor). If it does not,
   the plan says to use the sliders button of the step.
4. **The Fields and Data tabs** of the step read as the plan says: **Produced** lists 12 fields ending with **Roles**, and
   **Preview** shows three rows. The text of the tabs and of the table was read from `FieldsTab.vue` and
   `PreviewTab.vue`, not seen.
5. **Optional:** that a user without the **View user profiles** permission (`ViewUsers`) does not get the **Users**
   cards. The narration of `consume-3` says the Users data source lists its data sets only to users who may view users
   (`UsersDataSource.cs:163-166`).

## On the slides

6. **The code is not compiled.** The slides trim the examples of the Data Sources documentation (`Support`,
   `TicketsDataSet`, `WarehouseDataSource`, the change queue). They use types that the documentation leaves undefined
   (`ITicketStore`, `TicketState`, `SupportPermissions`, `_ticketStore`, `PageSize`, `StockDataSet`, `ChangedOrder`,
   `OrderChangeQueue`) and a few lines that were joined or split to fit a panel (the signature of `LoadAsync`, the
   `Description` property, the `DataSetSchema` return). The members they call were checked against the source of
   `OrchardCore.DataSources.Abstractions` and `OrchardCore.DataSources.Core`: `RecordDataSource`, `RecordDataSet<TRecord>`
   (`AddField`, `CanReadAsync`, `LoadAsync`), `DataField.IsIdentifier`, `IsKeyFilterable`, `References`,
   `DataFieldReference`, `DataConditions.Keys` and `DateRange`, `IDataSource`, `DataBatch`, `DataSourceContext.OnSucceeded`
   and `ConsumerId`, `IDataAggregateSource`, `AddDataSource<T>()`. A person who knows the documentation's author may
   want them compiled once in a module.
7. **`[RequireFeatures("OrchardCore.DataPipelines.Core")]` on a startup without `[Feature]`** is the pattern of the
   documentation. The data sources that ship with the module use `[Feature(DataPipelinesConstants.CoreFeatureId)]` with
   `[RequireFeatures("OrchardCore.Users")]` (`Startups/CoreStartup.cs:42-60`). Both patterns are in the narration only as
   "a startup that requires the Data Pipelines Core feature".
8. **"Enabling your module enables it too"** (`build-1`) relies on the general rule that a feature enables its
   dependencies, which the documentation of the feature states. It was not run with a module of this kind.

## Spoken spellings to confirm

Per the skill, a person confirms each new spelling with `scripts/pronounce.py` before narrating (it makes audio, so it
was not run). `storyboard.json` maps these written identifiers to the words the voice says; the captions keep the
written form:

| Written | Spoken |
|---|---|
| `IDataAggregateSource` | I Data Aggregate Source |
| `IDataSource` | I Data Source |
| `RecordDataSource` | Record Data Source |
| `RecordDataSet` | Record Data Set |
| `AddDataSource` | Add Data Source |
| `AddField` | Add Field |
| `IsIdentifier` | Is Identifier |
| `IsKeyFilterable` | Is Key Filterable |
| `CanReadAsync` | Can Read Async |
| `LoadAsync` | Load Async |
| `GetDataSetsAsync` | Get Data Sets Async |
| `GetSchemaAsync` | Get Schema Async |
| `ReadAsync` | Read Async |
| `DataConditions` | Data Conditions |
| `OnSucceeded` | On Succeeded |
| `ConsumerId` | Consumer Id |

The entries are replaced in this order (`narrate.py` replaces plain text, in the order of the map). `ReadAsync` is a part
of `CanReadAsync`, which comes first, so its replacement leaves `Read Async` and no later entry changes it. Also listen to how the voice says `DisplayName`, `Description`, `References`, `.NET` and
`DateTime` in the narration.
