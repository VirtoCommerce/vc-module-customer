# Virto Commerce Customer Module

[![CI status](https://github.com/VirtoCommerce/vc-module-customer/workflows/Module%20CI/badge.svg?branch=dev)](https://github.com/VirtoCommerce/vc-module-customer/actions?query=workflow%3A"Module+CI") [![Quality gate](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer&metric=alert_status&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer) [![Reliability rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer&metric=reliability_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer) [![Security rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer&metric=security_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer) [![Sqale rating](https://sonarcloud.io/api/project_badges/measure?project=VirtoCommerce_vc-module-customer&metric=sqale_rating&branch=dev)](https://sonarcloud.io/dashboard?id=VirtoCommerce_vc-module-customer)

## Overview

The Customer module is Virto Commerce's contacts management system. It stores and manages the people and companies that interact with a Virto Commerce solution — store customers, employees, partner organisations, and third-party vendors — in a single polymorphic hierarchy.

The module provides a complete back-office experience (the **Companies and contacts** blade with a `<va-filter-panel>`-powered filter for member type and created/modified date ranges), a REST API for polymorphic CRUD and search, pluggable search indexing, integration events for downstream modules, and fine-grained permissions.

## Key features

* **Polymorphic member hierarchy** — `Contact`, `Organization`, `Employee`, `Vendor` inherit from a shared `Member` base and live in a parent/child tree (companies can contain sub-companies, employees, and contacts).
* **Extensible member types** — additional types can be registered by downstream modules via `memberTypesResolverService.registerType(...)` on the UI side and the polymorphic JSON converter on the API side.
* **Polymorphic REST API** — a single `/api/members` surface handles search/CRUD for every type, with type-specific convenience routes (`/contacts`, `/organizations`, `/vendors`, `/employees`).
* **Structured filter panel** — filter members by type and Created/Modified date ranges (preset + custom) using Lucene-style keyword syntax (`membertype:Organization`, `createddate:[2026-01-01 TO]`).
* **Search indexing** — member documents are indexed through the Search module with event-driven reindexing on create/update/delete.
* **Invite customers** — generate invitation emails that onboard external contacts with a specific role (`RegisterCompanyEmailNotification` template).
* **Dynamic properties & SEO** — all member types support dynamic properties; `Vendor` also ships with SEO widget integration.
* **Favourite addresses and customer preferences** — per-member preference storage with its own CRUD/search services.
* **Multi-database support** — ships adapters for SQL Server, MySQL, and PostgreSQL.
* **Organization-scoped permissions** — `AssociatedOrganizationsOnlyScope` restricts access to members belonging to the caller's organisation.

## Architecture

The module follows the standard Virto Commerce three-layer split (Core → Data → Web) with pluggable database adapters.

```
VirtoCommerce.CustomerModule.Core         — contracts: domain models, service interfaces, events, constants
VirtoCommerce.CustomerModule.Data         — EF Core implementation, repositories, handlers, indexing, import/export
VirtoCommerce.CustomerModule.Data.SqlServer
VirtoCommerce.CustomerModule.Data.MySql   — database-provider adapters (EF Core migrations per provider)
VirtoCommerce.CustomerModule.Data.PostgreSql
VirtoCommerce.CustomerModule.Web          — HTTP API controllers + AngularJS admin UI (blades, directives, i18n)
```

### Domain model (`Core/Model/`)

`Member` (abstract, extends `AuditableEntity`) is the polymorphic root, discriminated by `MemberType`. Concrete types:

| Type | Notable interfaces/fields |
|------|---------------------------|
| `Contact` | `IHasSecurityAccounts`, `IHasPersonName`, `IHasOrganizations` — associated organisations, birthday, preferred language |
| `Organization` | Hierarchy via `ParentId` + `OwnerId`, business category |
| `Employee` | `IHasSecurityAccounts`, `IHasPersonName`, `IHasOrganizations` — `IsActive`, `EmployeeType`, default organisation |
| `Vendor` | `IHasSecurityAccounts` — site URL, logo, group name |

Every member carries collections of `Address`, `Phone`, `Email`, `Note`, `Group`, plus dynamic properties. Supporting models live alongside: `CustomerPreference`, `CustomerRole`, `RelationType`.

### Services (`Core/Services/`)

| Service | Purpose |
|---------|---------|
| `IMemberService` | Polymorphic CRUD (`GetByIdsAsync`, `SaveChangesAsync`, `DeleteAsync`) |
| `IMemberSearchService` | Keyword + filter-based search (`SearchMembersAsync`) — powers the admin list blade |
| `IMemberResolver` | Maps a platform user ID to the member record linked through security accounts |
| `IFavoriteAddressService` | Per-member favourite addresses |
| `ICustomerPreferenceService`, `ICustomerPreferenceCrudService`, `ICustomerPreferenceSearchService` | Named preferences keyed by member |
| `IInviteCustomerService` | Invitation flow: send email + enumerate available roles |

### HTTP API (`Web/Controllers/Api/`)

`CustomerModuleController` serves `/api/members` (polymorphic) with convenience aliases at `/api/contacts`, `/api/organizations`, `/api/vendors`, `/api/employees`:

* `POST /members/search` — keyword + filter search
* `GET /members/{id}` · `GET /members?ids=…` · `GET /members/organizations`
* `POST /members` · `PUT /members` · `PATCH /members/{id}` · `POST /members/bulk` · `PUT /members/bulk`
* `DELETE /members` · `POST /members/delete` (delete-by-criteria)
* `GET /members/accounts/{userId}` · `GET /members/{id}/organizations`
* `PUT /addresses`
* `POST /members/customers/invite` · `GET /members/customers/invite/roles`

`CustomerPreferenceController` serves `/api/customer-preferences` with search, CRUD, and delete endpoints.

### Events (`Core/Events/`)

All inherit `GenericChangedEntryEvent<T>`:

* `MemberChangingEvent` / `MemberChangedEvent` — before/after any member CRUD
* `CustomerPreferenceChangingEvent` / `CustomerPreferenceChangedEvent` — before/after preference CRUD

The module also handles `UserChangedEvent`, `UserRoleAddedEvent`, `UserRoleRemovedEvent` to keep member security-account links in sync.

### Web integration points

* **Main menu** — "Contacts" entry (priority 180, permission `customer:access`).
* **Member-type registry** — AngularJS `memberTypesResolverService` registers built-in types and exposes the list used by both `member-add` and the filter panel.
* **Widgets** — dynamic-properties widget on every detail blade; address/email/phone widgets; search-index widget; vendor SEO widget; account-contact widget on user-profile blade.
* **Permission scope** — `AssociatedOrganizationsOnlyScope` registered with the platform scope resolver.
* **Polymorphic serialisation** — `PolymorphJsonConverter` switches on `Member.MemberType`.
* **Notification template** — `RegisterCompanyEmailNotification` (extends `EmailNotification`) used by the invite flow.

## Configuration

### Database provider

Set via the platform's `DatabaseProvider` config key (`appsettings.json` or environment variable). Allowed values: `SqlServer` (default), `MySql`, `PostgreSql`. The matching `VirtoCommerce.CustomerModule.Data.<Provider>` adapter must be deployed alongside the module.

### Module settings (`Core/ModuleConstants.cs`)

All settings live under the `Customer` group:

| Setting | Type | Default | Purpose |
|---------|------|---------|---------|
| `Customer.MemberGroups` | dict | `New` (allowed: `VIP`, `Wholesaler`) | Tags available on members |
| `Customer.ContactStatuses` / `OrganizationStatuses` / `VendorStatuses` / `EmployeeStatuses` | dict | `New` (allowed: `New`, `Approved`, `Rejected`, `Deleted`) | Status dropdown values per type |
| `Customer.ContactDefaultStatus` · `Customer.OrganizationDefaultStatus` | string | — | Per-store default status |
| `Customer.ExportImport.PageSize` | int | `50` | Batch size for bulk export/import |
| `Customer.Search.EventBasedIndexation.Enable` | bool | `true` | Reindex members on `MemberChangedEvent` |
| `VirtoCommerce.Search.IndexingJobs.IndexationDate.Member` | datetime | — | Last-successful indexation timestamp (maintained by jobs) |

### Permissions (`Core/ModuleConstants.cs`)

Registered under scope `Customer`:

| Permission | Grants |
|------------|--------|
| `customer:access` | List/enumerate members |
| `customer:read` | View member details |
| `customer:create` | Create new members |
| `customer:update` | Edit members |
| `customer:delete` | Delete members |
| `customer:invite` | Send customer invitations |

Scope `AssociatedOrganizationsOnlyScope` can be attached to any of the above to limit visibility to members inside the caller's organisations.

## Documentation

* [Customer module user documentation](https://docs.virtocommerce.org/platform/user-guide/contacts/overview/)
* [REST API](https://virtostart-demo-admin.govirto.com/docs/index.html?urls.primaryName=VirtoCommerce.Customer)
* [View on Github](https://github.com/VirtoCommerce/vc-module-customer)
* [Developer guide](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/extending-domain-models/)

## References

* [Deployment](https://docs.virtocommerce.org/platform/developer-guide/Tutorials-and-How-tos/Tutorials/deploy-module-from-source-code/)
* [Installation](https://docs.virtocommerce.org/platform/user-guide/modules-installation/)
* [Home](https://virtocommerce.com)
* [Community](https://www.virtocommerce.org)
* [Download latest release](https://github.com/VirtoCommerce/vc-module-customer/releases/latest)

## Available resources

* Module related service implementations as a <a href="https://www.nuget.org/packages/VirtoCommerce.CustomerModule.Data" target="_blank">NuGet package</a>
* API client as a <a href="https://www.nuget.org/packages/VirtoCommerce.CustomerModule.Client" target="_blank">NuGet package</a>

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
