USE AtlasDb;

insert into customers (name, description, isactive, createdat, updatedat)
Values
('Altice','Altice DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Charter','Charter DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Comcast Mid Market','Comcast Mid Market DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Comcast Residential','Comcast Residential DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Cricket','Cricket DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Metro','Metro DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Service Bureau','Service Bureau DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('USCC','USCC DCs',1,'2025-11-20 14:48:00','2025-11-20 14:48:00')
;

insert into environments (name, description, isactive, createdat, updatedat)
values
('PROD','Production',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Non-PROD','Non-Production',1,'2025-11-20 14:48:00','2025-11-20 14:48:00')
;

insert into roles (name, description, iscritical, isactive, createdat, updatedat)
values
('DC','Domain Controller',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Gate','Gate Server',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Web','WebServer',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('WS','Workstation',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('Other','Some other use',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00')
;

insert into locations (name, type, description, isactive, createdat, updatedat)
values
('CMI','Data Center','Champaign DC',1,'2025-11-20 14:48:00','2025-11-20 14:48:00'),
('SFR','Data Center','Moses Lake DC',1,'2025-11-20 14:48:00','2025-11-20 14:48:00')
;

insert into vcenters (Name, IpAddress, Description, Passphrase, IsActive, CreatedAt, UpdatedAt)
Values
('AIO/Cricket Prod','10.204.1.240','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('AIO/Cricket PLAB ACE','10.204.6.200','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('AIO/Cricket 100% DR','10.204.39.135','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('MetroPCS Ewallet Production','10.99.50.60','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('MetroPCS Ewallet PLAB','10.99.169.37','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Altice Prod-2','10.97.99.101','List-Of-VMs','VXRail',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Altice DR-2','10.97.199.101','List-Of-VMs','VXRail',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Service Bureau CMI','10.124.90.55','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Service Bureau CMI','10.124.1.178','','BSO',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Service Bureau DR','10.123.30.15','','BSO',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('CMD Modesto Prod VC02','10.109.4.11','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Comcast Enterprise Prod','10.124.6.60','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Modesto Non Prod VC02','10.109.132.11','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('USCC Prod (CMI)','10.100.14.101','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('USCC Non-Prod (SFR)','10.100.142.101','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Cricket Vxrail Prod','10.204.19.167','List-Of-VMs','VXRail',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Charter Production CMI','10.112.19.100','List-Of-VMs','VXRail',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Comcast OCP UAT','10.123.57.200','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00'),
('Comcast OCP PROD','10.105.64.50','List-Of-VMs','Pluto',1,'2025-11-21 10:48:00','2025-11-21 10:48:00')
;

insert into servers (hostname, ipaddress, OsName, OsFamily, isvm, isactive, createdat, updatedat, environmentid, roleid, locationid, customerid, vcenterid)
values
('cmisbaudit01','10.124.1.103','Windows Server','2016 or later (64 bit)',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00',3,10,3,15,9),
('SFRALTAUTO01','10.97.204.80','Windows Server','2016 or later (64 bit)',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00',4,10,4,9,7),
('SFRMPCSPLAB01','10.99.161.147','Windows Server','2008R2 (64 bit)',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00',4,10,4,14,10)
;

