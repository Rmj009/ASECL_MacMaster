# Create New User
CREATE USER IF NOT EXISTS 'root'@'%' IDENTIFIED BY '11111111';
GRANT ALL PRIVILEGES ON *.* TO 'root'@'%' WITH GRANT OPTION;

CREATE USER IF NOT EXISTS 'toyota'@'%' IDENTIFIED BY '11111111';
GRANT ALL PRIVILEGES ON *.* TO 'toyota'@'%' WITH GRANT OPTION;

CREATE USER IF NOT EXISTS 'toyota'@'localhost' IDENTIFIED BY '11111111';
GRANT ALL PRIVILEGES ON *.* TO 'toyota'@'localhost' WITH GRANT OPTION;

CREATE USER IF NOT EXISTS 'replication'@'%' IDENTIFIED BY '11111111';
GRANT REPLICATION SLAVE ON *.* TO 'replication'@'%';
GRANT BACKUP_ADMIN ON *.* TO 'replication'@'%';
GRANT CLONE_ADMIN ON *.* TO 'replication'@'%';

# Create Database And Auth
# database using utf8_general_cs --> case sensitive, utf8mb4_unicode_ci --> case insensitive
CREATE DATABASE IF NOT EXISTS ASECL_SIMDC_SERVER DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
GRANT ALL PRIVILEGES ON ASECL_SIMDC_SERVER.* TO 'root'@'localhost';
GRANT ALL PRIVILEGES ON ASECL_SIMDC_SERVER.* TO 'root'@'%';
GRANT ALL PRIVILEGES ON ASECL_SIMDC_SERVER.* TO 'toyota'@'localhost';
GRANT ALL PRIVILEGES ON ASECL_SIMDC_SERVER.* TO 'toyota'@'%';

# Flush DB
FLUSH PRIVILEGES;

# SHOW GRANTS FOR 'toyota'@'%';

use ASECL_SIMDC_SERVER;


# Add Store Procedure
DELIMITER $$

DROP PROCEDURE IF EXISTS `ASECL_SIMDC_SERVER`.`CreateIndex` $$
CREATE PROCEDURE `ASECL_SIMDC_SERVER`.`CreateIndex`
(
    given_database VARCHAR(64),
    given_table    VARCHAR(64),
    given_index    VARCHAR(64),
    given_columns  VARCHAR(64)
)
BEGIN

    DECLARE IndexIsThere INTEGER;

    SELECT COUNT(1) INTO IndexIsThere
    FROM INFORMATION_SCHEMA.STATISTICS
    WHERE table_schema = given_database
    AND   table_name   = given_table
    AND   index_name   = given_index;

    IF IndexIsThere = 0 THEN
        SET @sqlstmt = CONCAT('CREATE INDEX ',given_index,' ON ',
        given_database,'.',given_table,' (',given_columns,')');
        PREPARE st FROM @sqlstmt;
        EXECUTE st;
        DEALLOCATE PREPARE st;
   -- ELSE
     --   SELECT CONCAT('Index ',given_index,' already exists on Table ',
    --    given_database,'.',given_table) CreateindexErrorMessage;   
    END IF;

END $$

DELIMITER ;


# Create Table ROLE
CREATE TABLE IF NOT EXISTS `Role` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `RoleNameIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# Create User
CREATE TABLE IF NOT EXISTS `User` (
   `ID` BIGINT NOT NULL AUTO_INCREMENT,
   `EmployeeID` VARCHAR(11) NOT NULL DEFAULT '00000',
   `RealName` NVARCHAR(255) NOT NULL DEFAULT '',
   `NickName` NVARCHAR(255) NOT NULL DEFAULT '',
   `EMail` VARCHAR(100) NOT NULL DEFAULT '',
   `Phone` VARCHAR(100) NOT NULL DEFAULT '',
   `Role_ID` BIGINT,
   FOREIGN KEY(Role_ID) REFERENCES Role(ID) ON DELETE SET NULL ON UPDATE CASCADE,
   `Password` NVARCHAR(255) NOT NULL DEFAULT '',
   `AgreeUser_ID` BIGINT,
   FOREIGN KEY(AgreeUser_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
   `DisagreeUser_ID` BIGINT,
   FOREIGN KEY(DisagreeUser_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
   `IsActived` smallint(1) NOT NULL DEFAULT 0,
   `DisagreeReason` NVARCHAR(255) NOT NULL DEFAULT '',
   `LastActivedTime` TIMESTAMP,
   `LastDisagreeActiveTime` TIMESTAMP,
   PRIMARY KEY (`ID`),
   UNIQUE KEY `RealNameIndex` (`RealName`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'User', 'UserNickNameIndex', 'NickName');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'User', 'UserEMailIndex', 'EMail');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'User', 'UserPhoneIndex', 'Phone');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'User', 'UserIsActivedIndex', 'IsActived');


# RegisterType
CREATE TABLE IF NOT EXISTS `RegisterType` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `RegisterTypeIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# Register
CREATE TABLE IF NOT EXISTS `Register` (
   `ID` BIGINT NOT NULL AUTO_INCREMENT,
   `RegisterUser_ID` BIGINT,
   FOREIGN KEY(RegisterUser_ID) REFERENCES User(ID) ON DELETE CASCADE ON UPDATE CASCADE,
   `RegisterType_ID` BIGINT,
   FOREIGN KEY(RegisterType_ID) REFERENCES registertype(ID) ON DELETE SET NULL ON UPDATE CASCADE,
   `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
   PRIMARY KEY (`ID`),
   UNIQUE KEY `RegisterUserIDIndex` (`RegisterUser_ID`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# LoginType
CREATE TABLE IF NOT EXISTS `LoginType` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `LoginTypeIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


-- # IC
-- CREATE TABLE IF NOT EXISTS `IC` (
--     `ID` BIGINT NOT NULL AUTO_INCREMENT,
--     `Name` NVARCHAR(255) NOT NULL DEFAULT '',
--     `Remark` NVARCHAR(255) NOT NULL DEFAULT '',
--      `CreatedOwner_ID` BIGINT,
--     FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
--     `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     PRIMARY KEY (`ID`),
--     UNIQUE KEY `ICIndex` (`Name`) USING HASH
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# FW
CREATE TABLE IF NOT EXISTS `Firmware` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL,
    `Version` VARCHAR(255),
    `Path` NVARCHAR(255),
    `MD5` VARCHAR(255),
    `Remark` NVARCHAR(255),
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `FirmwareIndex` (`Name`, `Version`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Firmware', 'FirmwareNameIndex', 'Name');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Firmware', 'FirmwarePathIndex', 'Path');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Firmware', 'FirmwareCreatedTimeIndex', 'CreatedTime');

# Customer
CREATE TABLE IF NOT EXISTS `Customer` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `Phone` VARCHAR(100) NOT NULL DEFAULT '',
    `Remark` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `CustomerNameIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Customer', 'CustomerCreatedTimeIndex', 'CreatedTime');


# SW
CREATE TABLE IF NOT EXISTS `Software` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL,
    `Version` VARCHAR(255) NOT NULL,
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `SoftwareIndex` (`Name`, `Version`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Software', 'SoftwareNameIndex', 'Name');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Software', 'SoftwareCreatedTimeIndex', 'CreatedTime');


-- # Software_Binding
-- CREATE TABLE IF NOT EXISTS `Software_Binding` (
--     `ID` BIGINT NOT NULL AUTO_INCREMENT,
-- 	`SW_ID` BIGINT,
--     FOREIGN KEY(SW_ID) REFERENCES Software(ID) ON DELETE CASCADE ON UPDATE CASCADE,
--     `FW_ID` BIGINT,
--     FOREIGN KEY(FW_ID) REFERENCES Firmware(ID) ON DELETE SET NULL ON UPDATE CASCADE,
--      `CreatedOwner_ID` BIGINT,
--     FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
--     `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     PRIMARY KEY (`ID`)
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

# Product_Family
CREATE TABLE IF NOT EXISTS `Product_Family` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `Remark` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `Product_FamilyIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_Family', 'Product_FamilyCreatedTimeIndex', 'CreatedTime');


# ProductDevice
CREATE TABLE IF NOT EXISTS `ProductDevice` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `Remark` NVARCHAR(255),
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
	`Product_Family_ID` BIGINT,
    FOREIGN KEY(Product_Family_ID) REFERENCES Product_Family(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `ProductIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'ProductDevice', 'ProductCreatedTimeIndex', 'CreatedTime');


# TestConfigurationStatus
CREATE TABLE IF NOT EXISTS `TestConfigurationStatus` (
     `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `TestConfiguration_StatusIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfigurationStatus', 'TestConfigurationStatusNameIndex', 'Name');


# MacDispatchType
CREATE TABLE IF NOT EXISTS `MacDispatchType` (
     `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `MacDispatchTypeIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacDispatchType', 'MacDispatchTypeNameIndex', 'Name');

# TestConfiguration
CREATE TABLE IF NOT EXISTS `TestConfiguration` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `LotCode` NVARCHAR(255) NOT NULL DEFAULT '',
    `PID` NVARCHAR(255) NOT NULL DEFAULT '',
    `Status_ID` BIGINT,
    FOREIGN KEY(Status_ID) REFERENCES TestConfigurationStatus(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
     `ProductDevice_ID` BIGINT,
    FOREIGN KEY(ProductDevice_ID) REFERENCES ProductDevice(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `DispatchType_ID` BIGINT,
    FOREIGN KEY(DispatchType_ID) REFERENCES MacDispatchType(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
	`Customer_ID` BIGINT,
	`ForceStopOPId` NVARCHAR(255),
    FOREIGN KEY(Customer_ID) REFERENCES Customer(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `ForceStopUser_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
	`ForceStopRemark` NVARCHAR(4096),
	`FinishDate` TIMESTAMP,
	`ExtraJson` JSON,
	`ExtraJsonOPId` NVARCHAR(255) GENERATED ALWAYS AS (ExtraJson->"$.opId"),
	`ExtraJsonDutTestMode` NVARCHAR(255) GENERATED ALWAYS AS (ExtraJson->"$.dutTestMode"),
	`LogTitle` JSON,
    `LogLimitUpper` JSON,
    `LogLimitLower` JSON,
    `TrayMode` SMALLINT(1) default 0,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `TestLotCodeIndex` (`LotCode`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestIsActiveIndex', 'IsActive');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationIndex', 'LotCode,ProductDevice_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationCreatedTimeIndex', 'CreatedTime');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationFinishDateIndex', 'FinishDate');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationExtraJsonIndex', 'ExtraJsonOPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationExtraJsonDutTestModeIndex', 'ExtraJsonDutTestMode');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationForceStopOPIdIndex', 'ForceStopOPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationTrayModeIndex', 'TrayMode');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestConfigurationPIDIndex', 'PID');


# TestConfiguration_SW_FW_Binding
CREATE TABLE IF NOT EXISTS `TestConfiguration_SW_FW_Binding` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
	`TestConfiguration_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
	`FW_ID` BIGINT,
    FOREIGN KEY(FW_ID) REFERENCES Firmware(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `SW_ID` BIGINT,
    FOREIGN KEY(SW_ID) REFERENCES Software(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
	`IsActived` smallint(1) NOT NULL DEFAULT 0,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration_SW_FW_Binding', 'TestConfiguration_SW_FW_Binding_FW_ID', 'FW_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration_SW_FW_Binding', 'TestConfiguration_SW_FW_Binding_SW_ID', 'SW_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration_SW_FW_Binding', 'TestConfiguration_SW_FW_Binding_IsActived', 'IsActived');


# TestConfiguration_SW_FW_Binding_Log
CREATE TABLE IF NOT EXISTS `TestConfiguration_SW_FW_Binding_Log` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
	`TestConfiguration_SW_FW_Binding_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_SW_FW_Binding_ID) REFERENCES TestConfiguration_SW_FW_Binding(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
	`OPId` NVARCHAR(255) NOT NULL DEFAULT '',
    `TestFlow` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration_SW_FW_Binding_Log', 'TestConfiguration_SW_FW_Binding_Log_OPId', 'OPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration_SW_FW_Binding_Log', 'TestConfiguration_SW_FW_Binding_Log_TestFlow', 'TestFlow');




# Login
CREATE TABLE IF NOT EXISTS `Login` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
	`LoginUser_ID` BIGINT,
    FOREIGN KEY(LoginUser_ID) REFERENCES User(ID) ON DELETE CASCADE ON UPDATE CASCADE,
    `JwtToken` VARCHAR(255) NOT NULL DEFAULT '',
	`LoginType_ID` BIGINT,
    FOREIGN KEY(LoginType_ID) REFERENCES LoginType(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `LastModifyTime` TIMESTAMP,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `LoginJwtIndex` (`JwtToken`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# MacType
CREATE TABLE IF NOT EXISTS `MacType` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `MacTypeIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacType', 'MacTypeNameIndex', 'Name');


# MacStatus
CREATE TABLE IF NOT EXISTS `MacStatus` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `MacTypeIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacStatus', 'MacStatusNameIndex', 'Name');


# MacAddress
CREATE TABLE IF NOT EXISTS `MacAddress` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` VARCHAR(255),
    `Address` VARCHAR(255) NOT NULL DEFAULT '',
	`AddressDecimal` BIGINT NOT NULL DEFAULT 0,
	`Status_ID` BIGINT,
    FOREIGN KEY(Status_ID) REFERENCES MacStatus(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `MacType_ID` BIGINT,
    FOREIGN KEY(MacType_ID) REFERENCES MacType(ID) ON DELETE SET NULL ON UPDATE CASCADE,
	`TestConfiguration_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE SET NULL ON UPDATE CASCADE,
	`Testuser_ID` BIGINT,
    FOREIGN KEY(Testuser_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedOwner_ID` BIGINT,
	FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
	UNIQUE KEY `MacAddressDecimalIndex` (`AddressDecimal`, `MacType_ID`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress', 'MacAddressNameIndex', 'Name');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress', 'MacAddressCreatedTimeIndex', 'CreatedTime');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress', 'MacAddressAddressDecimalIndex', 'AddressDecimal');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress', 'MacAddressAddressIndex', 'Address');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress', 'MacAddressTestConfiguration_IDIndex', 'TestConfiguration_ID');


# TrayType
CREATE TABLE IF NOT EXISTS `TrayType` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `TrayTypeIndex` (`Name`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# Tray
CREATE TABLE IF NOT EXISTS `Tray` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `TestConfiguration_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
     `TrayType_ID` BIGINT,
    FOREIGN KEY(TrayType_ID) REFERENCES TrayType(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `Width` BIGINT NOT NULL,
    `Height` BIGINT NOT NULL,
    `Order` BIGINT NOT NULL,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Tray', 'OrderIndex', 'Order');



-- # ICFunction
-- CREATE TABLE IF NOT EXISTS `ICFunction` (
--     `ID` BIGINT NOT NULL AUTO_INCREMENT,
--     `Name` NVARCHAR(255) NOT NULL DEFAULT '',
--     `Remark` NVARCHAR(255) NOT NULL DEFAULT '',
--      `CreatedOwner_ID` BIGINT,
--     FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
--     `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     PRIMARY KEY (`ID`),
--     UNIQUE KEY `ICFuncIndex` (`Name`) USING HASH
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
--
--
-- # ICBinding
-- CREATE TABLE IF NOT EXISTS `ICBinding` (
--     `ID` BIGINT NOT NULL AUTO_INCREMENT,
-- 	`IC_ID` BIGINT,
--     FOREIGN KEY(IC_ID) REFERENCES IC(ID) ON DELETE CASCADE ON UPDATE CASCADE,
-- 	`ICFunction_ID` BIGINT,
--     FOREIGN KEY(ICFunction_ID) REFERENCES ICFunction(ID) ON DELETE SET NULL ON UPDATE CASCADE,
-- 	`CreatedOwner_ID` BIGINT,
--     FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
--     `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--     PRIMARY KEY (`ID`)
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;




-- # Product_Binding
-- CREATE TABLE IF NOT EXISTS `Product_Binding` (
--     `ID` BIGINT NOT NULL AUTO_INCREMENT,
--     `Product_ID` BIGINT,
--     FOREIGN KEY(Product_ID) REFERENCES Product(ID) ON DELETE CASCADE ON UPDATE CASCADE,
--     `TestConfiguration_ID` BIGINT,
--     FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE SET NULL ON UPDATE CASCADE,
--     PRIMARY KEY (`ID`)
-- ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# DutDevice
CREATE TABLE IF NOT EXISTS `DutDevice` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
	`HostName` VARCHAR(255),
	`ProductDevice` VARCHAR(255) NOT NULL DEFAULT '',
    `Remark` NVARCHAR(255) NOT NULL DEFAULT '',
	`GroupPC` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedOwner_ID` BIGINT,
    FOREIGN KEY(CreatedOwner_ID) REFERENCES User(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `DutDeviceIPNameIndex` (`HostName`, `ProductDevice`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'DutDevice', 'DeviceGroupPCIndexOwnerIndex', 'GroupPC');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'DutDevice', 'DeviceProductDeviceIndexOwnerIndex', 'ProductDevice');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'DutDevice', 'DeviceCreatedTimeIndexOwnerIndex', 'CreatedTime');


# TestConfiguration_DutBinding
CREATE TABLE IF NOT EXISTS `TestConfiguration_DutBinding` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `DutDevice_ID` BIGINT,
    FOREIGN KEY(DutDevice_ID) REFERENCES DutDevice(ID) ON DELETE CASCADE ON UPDATE RESTRICT ,
    `TestConfiguration_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration_DutBinding', 'TestConfiguration_DutBindingCreatedTimeIndexOwnerIndex', 'CreatedTime');

# TestResultStatus
CREATE TABLE IF NOT EXISTS `TestResultStatus` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Result` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `TestResultIndex` (`Result`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestResultStatus', 'ResultIndex', 'Result');


# MacAddress_ResultBinding
CREATE TABLE IF NOT EXISTS `MacAddress_ResultBinding` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `OPId` NVARCHAR(255) NOT NULL DEFAULT '',
    `TestFlow` NVARCHAR(255) NOT NULL DEFAULT '',
    `DutDevice_ID` BIGINT,
    FOREIGN KEY(DutDevice_ID) REFERENCES DutDevice(ID) ON DELETE CASCADE ON UPDATE CASCADE ,
    `Mac_ID` BIGINT,
    FOREIGN KEY(Mac_ID) REFERENCES MacAddress(ID) ON DELETE CASCADE ON UPDATE CASCADE ,
    `ResultStatus_ID` BIGINT,
    FOREIGN KEY(ResultStatus_ID) REFERENCES TestResultStatus(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	`ResultSummary` JSON,
	`Path` NVARCHAR(255),
	`SipSerialName` VARCHAR(255),
	`2DBarcode` VARCHAR(255),
	`2DBarcode_Vendor` VARCHAR(255),
	`TestConfiguration_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress_ResultBinding', 'MacAddress_ResultBindingMac_IDIndexOwnerIndex', 'Mac_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress_ResultBinding', 'MacAddress_ResultBindingDutDevice_IDIndexOwnerIndex', 'DutDevice_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress_ResultBinding', 'MacAddress_ResultBindingOPIdIndex', 'OPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress_ResultBinding', 'MacAddress_ResultBindingTestFlowIndex', 'TestFlow');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress_ResultBinding', 'MacAddress_ResultBindingSipSerialNameIndex', 'SipSerialName');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'MacAddress_ResultBinding', 'MacAddress_ResultBinding2DBarcode_VendorIndex', '2DBarcode_Vendor');



# TrayPosition
CREATE TABLE IF NOT EXISTS `TrayPosition` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
     `TestConfiguration_ID` BIGINT,
    FOREIGN KEY(TestConfiguration_ID) REFERENCES TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
     `MacAddress_ResultBinding_ID` BIGINT,
    FOREIGN KEY(MacAddress_ResultBinding_ID) REFERENCES MacAddress_ResultBinding(ID) ON DELETE CASCADE ON UPDATE RESTRICT ,
      `Tray_ID` BIGINT,
    FOREIGN KEY(Tray_ID) REFERENCES Tray(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
     `MatrixIndex` BIGINT NOT NULL,
     `X` BIGINT NOT NULL,
     `Y` BIGINT NOT NULL,
     `ErrorCode` NVARCHAR(255) ,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TrayPosition', 'TrayPositionMac_IDIndexMatrixIndexIndex', 'MatrixIndex');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TrayPosition', 'TrayPositionErrorCodeIndex', 'ErrorCode');


# --- Product -------------------------------------------------------------
CREATE TABLE IF NOT EXISTS `Product_TestConfiguration` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `LotCode` NVARCHAR(255) NOT NULL DEFAULT '',
    `PID` NVARCHAR(255) NOT NULL DEFAULT '',
	`PO` NVARCHAR(255) NOT NULL DEFAULT '',
    `Status_ID` BIGINT,
    FOREIGN KEY(Status_ID) REFERENCES TestConfigurationStatus(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
     `ProductDevice_ID` BIGINT,
    FOREIGN KEY(ProductDevice_ID) REFERENCES ProductDevice(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
	`Customer_ID` BIGINT,
    FOREIGN KEY(Customer_ID) REFERENCES Customer(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `ForceStopOPId` NVARCHAR(255),
	`ForceStopRemark` NVARCHAR(4096),
	`FinishDate` TIMESTAMP,
	`ExtraJson` JSON,
	`ExtraJsonOPId` NVARCHAR(255) GENERATED ALWAYS AS (ExtraJson->"$.opId"),
	`ExtraJsonDutTestMode` NVARCHAR(255) GENERATED ALWAYS AS (ExtraJson->"$.dutTestMode"),
	`LogTitle` JSON,
    `LogLimitUpper` JSON,
    `LogLimitLower` JSON,
    `TrayMode` SMALLINT(1) default 0,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
    UNIQUE KEY `TestConfigurationIndex` (`LotCode`, `ProductDevice_ID`) USING HASH,
	UNIQUE KEY `TestConfigurationLPOIndex` (`LotCode`, `PO`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
-- call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'TestConfiguration', 'TestIsActiveIndex', 'IsActive');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestLotCodeIndex', 'LotCode');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationCreatedTimeIndex', 'CreatedTime');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationFinishDateIndex', 'FinishDate');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationExtraJsonIndex', 'ExtraJsonOPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationExtraJsonDutTestModeIndex', 'ExtraJsonDutTestMode');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationForceStopOPIdIndex', 'ForceStopOPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationTrayModeIndex', 'TrayMode');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration', 'TestConfigurationPIDIndex', 'PID');



CREATE TABLE IF NOT EXISTS `Product_TestConfiguration_SW_FW_Binding` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
	`Product_TestConfiguration_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_ID) REFERENCES Product_TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
	`FW_ID` BIGINT,
    FOREIGN KEY(FW_ID) REFERENCES Firmware(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `SW_ID` BIGINT,
    FOREIGN KEY(SW_ID) REFERENCES Software(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
	`IsActived` smallint(1) NOT NULL DEFAULT 0,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;


# Product_TestConfiguration_SW_FW_Binding_Log
CREATE TABLE IF NOT EXISTS `Product_TestConfiguration_SW_FW_Binding_Log` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
	`Product_TestConfiguration_SW_FW_Binding_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_SW_FW_Binding_ID) REFERENCES Product_TestConfiguration_SW_FW_Binding(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
	`OPId` NVARCHAR(255) NOT NULL DEFAULT '',
    `TestFlow` NVARCHAR(255) NOT NULL DEFAULT '',
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration_SW_FW_Binding_Log', 'Product_TestConfiguration_SW_FW_Binding_Log_OPId', 'OPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration_SW_FW_Binding_Log', 'Product_TestConfiguration_SW_FW_Binding_Log_TestFlow', 'TestFlow');




CREATE TABLE IF NOT EXISTS `Product_TestConfiguration_DutBinding` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `DutDevice_ID` BIGINT,
    FOREIGN KEY(DutDevice_ID) REFERENCES DutDevice(ID) ON DELETE CASCADE ON UPDATE RESTRICT ,
    `Product_TestConfiguration_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_ID) REFERENCES Product_TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TestConfiguration_DutBinding', 'TestConfiguration_DutBindingCreatedTimeIndexOwnerIndex', 'CreatedTime');



CREATE TABLE IF NOT EXISTS `Product_MacAddress` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `LotCode` VARCHAR(255),
    `PO` VARCHAR(255),
    `Name` VARCHAR(255),
    `Address` VARCHAR(255) NOT NULL DEFAULT '',
	`AddressDecimal` BIGINT NOT NULL DEFAULT 0,
	`Status_ID` BIGINT,
    FOREIGN KEY(Status_ID) REFERENCES MacStatus(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `MacType_ID` BIGINT,
    FOREIGN KEY(MacType_ID) REFERENCES MacType(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `Product_TestConfiguration_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_ID) REFERENCES Product_TestConfiguration(ID) ON DELETE SET NULL ON UPDATE CASCADE,
	`SipSerialName` VARCHAR(255),
    `SipLicense` VARCHAR(512),
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`),
	UNIQUE KEY `MacAddressDecimalIndex` (`AddressDecimal`, `MacType_ID`) USING HASH,
	UNIQUE KEY `SipSerialNameIndex` (`SipSerialName`) USING HASH,
	UNIQUE KEY `MacAddressLotCodePOAddressIndex` (`LotCode`, `PO`, `Address`) USING HASH
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressNameIndex', 'Name');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressCreatedTimeIndex', 'CreatedTime');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressAddressDecimalIndex', 'AddressDecimal');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressAddressIndex', 'Address');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressSipLicenseIndex', 'SipLicense');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressLotCodePOIndex', 'LotCode,PO');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress', 'MacAddressTestConfiguration_IDIndex', 'Product_TestConfiguration_ID');



CREATE TABLE IF NOT EXISTS `Product_Tray` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `Name` NVARCHAR(255) NOT NULL DEFAULT '',
    `Product_TestConfiguration_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_ID) REFERENCES Product_TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
     `TrayType_ID` BIGINT,
    FOREIGN KEY(TrayType_ID) REFERENCES TrayType(ID) ON DELETE RESTRICT ON UPDATE RESTRICT,
    `Width` BIGINT NOT NULL,
    `Height` BIGINT NOT NULL,
    `TOrder` BIGINT NOT NULL,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_Tray', 'OrderIndex', 'TOrder');



CREATE TABLE IF NOT EXISTS `Product_MacAddress_ResultBinding` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
    `OPId` NVARCHAR(255) NOT NULL DEFAULT '',
    `TestFlow` NVARCHAR(255) NOT NULL DEFAULT '',
    `DutDevice_ID` BIGINT,
    FOREIGN KEY(DutDevice_ID) REFERENCES DutDevice(ID) ON DELETE CASCADE ON UPDATE CASCADE ,
    `Product_MacAddress_ID` BIGINT,
    FOREIGN KEY(Product_MacAddress_ID) REFERENCES Product_MacAddress(ID) ON DELETE CASCADE ON UPDATE CASCADE ,
    `ResultStatus_ID` BIGINT,
    FOREIGN KEY(ResultStatus_ID) REFERENCES TestResultStatus(ID) ON DELETE CASCADE ON UPDATE CASCADE,
	`ResultSummary` JSON,
	`Path` NVARCHAR(255),
	`2DBarcode` VARCHAR(255),
	`2DBarcode_Vendor` VARCHAR(255),
	`Product_TestConfiguration_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_ID) REFERENCES Product_TestConfiguration(ID) ON DELETE SET NULL ON UPDATE CASCADE,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBindingMac_IDIndexOwnerIndex', 'Product_MacAddress_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBindingDutDevice_IDIndexOwnerIndex', 'DutDevice_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBindingOPIdIndex', 'OPId');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBindingTestFlowIndex', 'TestFlow');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBinding_Product_TestConfiguration_IDIndex', 'Product_TestConfiguration_ID');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBinding_2DBarcode_Index', '2DBarcode');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_MacAddress_ResultBinding', 'MacAddress_ResultBinding_2DBarcode_Vendor_Index', '2DBarcode_Vendor');



CREATE TABLE IF NOT EXISTS `Product_TrayPosition` (
    `ID` BIGINT NOT NULL AUTO_INCREMENT,
     `Product_TestConfiguration_ID` BIGINT,
    FOREIGN KEY(Product_TestConfiguration_ID) REFERENCES Product_TestConfiguration(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
     `Product_MacAddress_ResultBinding_ID` BIGINT,
    FOREIGN KEY(Product_MacAddress_ResultBinding_ID) REFERENCES Product_MacAddress_ResultBinding(ID) ON DELETE CASCADE ON UPDATE RESTRICT ,
      `Product_Tray_ID` BIGINT,
    FOREIGN KEY(Product_Tray_ID) REFERENCES Product_Tray(ID) ON DELETE CASCADE ON UPDATE RESTRICT,
     `MatrixIndex` BIGINT NOT NULL,
     `X` BIGINT NOT NULL,
     `Y` BIGINT NOT NULL,
     `ErrorCode` NVARCHAR(255) ,
    `CreatedTime` TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (`ID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TrayPosition', 'TrayPositionMac_IDIndexMatrixIndexIndex', 'MatrixIndex');
call `ASECL_SIMDC_SERVER`.`CreateIndex`('ASECL_SIMDC_SERVER', 'Product_TrayPosition', 'TrayPositionErrorCodeIndex', 'ErrorCode');



DELIMITER $$

DROP TRIGGER IF EXISTS `ASECL_SIMDC_SERVER`.`macaddress_when_del_testconfiguration` $$
CREATE TRIGGER `ASECL_SIMDC_SERVER`.`macaddress_when_del_testconfiguration` BEFORE DELETE ON testconfiguration
FOR EACH ROW
BEGIN
-- ERROR 1175: 1175: You are using safe update mode and you tried to update a table without a WHERE that uses a KEY column.
    SET SQL_SAFE_UPDATES=0;
-- 	DELETE from MacAddress_ResultBinding
-- 	WHERE MAC_ID in (select ID from macaddress where TestConfiguration_ID = OLD.ID );

    DELETE mr
    FROM MacAddress_ResultBinding as mr
    INNER JOIN macaddress as ma ON ma.ID = mr.MAC_ID
    WHERE ma.TestConfiguration_ID = OLD.ID;

--
    UPDATE macaddress
    SET
        Status_ID=(select ID from macstatus where Name = 'Unused'),
        Testuser_ID=null,
        Name=null
    WHERE TestConfiguration_ID = OLD.ID;
	SET SQL_SAFE_UPDATES=1;
END $$