# Virto Commerce Customer Sample Module

## Overview
The Virto Commerce Customer Sample Module demonstrates the extensibility framework for Customer modules' backend and Admin UI.

## Known limitations
* This sample is intended for demonstration purposes on a local machine.
* The module does not convert Contact to Contact2. To save the Job Title, you need to either create a new Contact or add a custom script to update the Discriminator from ContactEntity to Contact2Entity.
* The module is provided as a single project
* Is not compatible with the VC-BUILD utility.
* This module is designed to work with a SQL Server database.
* The provided _module.manifest is incorrect to prevent activation with Customer modules. 

## Features
This module includes the following features:

* Extends the Contact model by adding a new property, JobTitle.
* Introduces a new entity, Supplier.

## Screenshots
(Screenshots can be added here.)

## Getting Started
### Prerequisites

Before getting started with this module, ensure that you have the following prerequisites:
* Install and run the Virto Commerce platform on your local machine.
* Install the modules: ECommerce bundle.
* Install the B2B Sample Data.

### Setup
Follow these steps to set up the Virto Commerce Customer Sample Module:

1. Build the VirtoCommerce.CustomerSampleModule.Web project.

2. Build the Admin UI using the following commands:

```cmd
npm ci
```

```cmd
npm run webpack:dev
```

3. Rename the _module.manifest file to module.manifest.

4. Create a symbolic link from the VirtoCommerce.CustomerSampleModule.Web folder in the module's folder to the platform's modules folder. Use the mklink command, as shown below:

```cmd
mklink /D c:\vc-platform-3-demo\platform\modules\Dev.Customer.Sample c:\Projects\git\VirtoCommerce\vc-module-customer\samples\VirtoCommerce.CustomerSampleModule.Web
```

5. Run the Virto Commerce platform.

6. Verify that the Companies and Contacts Extension Sample (VirtoCommerce.CustomerSample) module loads correctly.
