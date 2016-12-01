# VSTS_MSID_TO_ORGID_MIGRATION

This repository is for development of a utility to migrate a Microsoft Account to Organizational Account (Work / School Account).

Generally when a VSTS is used in an Organization it is expected that only users from Organization have access to that particular VSTS instance. This can be enforced by having Active Directory integration and not allowing users from outside the AD to access the VSTS instance. Initially VSTS did not provide authentication using Organizational account and only Microsoft accounts were supported. Also as VSTS is free for unlimited MSDN subscribers this encouraged the addition of Microsoft accounts to VSTS. Any Organization will desire to have full control over the users and would not want any 3rd party entity to have access to its resources. 
  
Thus it is required to migrate the Microsoft account to Organization accounts. This can be done by assigning same permissions to Organization accounts as given to a person's Microsoft accounts. Also work item re-assignment is to be carried out. If there are numerous users and there can be numerous VSTS accounts in an Organization, it is not feasible to do this task manually.
 
Currently there is not proper tool available to achieve this task. Also the Client libraries and API's available cannot help us achieve all the objectives desired in this task. Therefore this project aims at development of a utility to automate migration of Microsoft accounts to Organization account using best tools available (client libraries / API's / console apps)
    
Current Progress in this utility:

i)	Adding users from AD to VSTS (Completed using client library)

ii)	Assigning Organizational users to project teams and giving them appropriate permissions and group memberships as given to same user's Microsoft account (Pending)

iii)	Re-assigning Work items from Microsoft Accounts to Organizational Accounts (Completed using REST API for VSTS)

iv)	Deleting Microsoft Accounts from VSTS (Pending)
