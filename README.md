# OS_GDPR

This plugin is designed to make OpenStore comply with European GDPR rules.  
European GDPR rules make retension and use of personal data restricted.  
Any personal data over 3 years, from an account that has not been active, should be removed from the database.  

#### User/Account Personal Data
Any users which have not be active for a selected amount of time (900 days default) will be listed for removal.  
The manager will then be able to decide to remove all users or selected users from the system.

NOTE: An option for automatic removal is available.  
NOTE: The orders are retained (See below) 

#### Orders
Orders also contain personal data. After an order is older than the remove limit (900 days default) the personal details are removed from the order.  
NOTE: The order is retained in the database for statistical and financial reports.  

#### Scheudler
##### IMPORTANT: The OpenStore DNN scheduler MUST be active to process this data automatically.

---
*Licensed under: GNU GENERAL PUBLIC LICENSE v3.*
