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

insert into servers (hostname, ipaddress, isvm, isactive, createdat, updatedat, environmentid, roleid, locationid, customerid)
values
('cmisbaudit01','10.124.1.103',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00',3,10,3,15),
('SFRALTAUTO01','10.97.204.80',1,1,'2025-11-20 14:48:00','2025-11-20 14:48:00',4,10,4,9)
;