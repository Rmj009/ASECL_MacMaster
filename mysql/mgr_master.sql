-- install PLUGIN group_replication SONAME 'group_replication.so';
-- SET GLOBAL disabled_storage_engines="MyISAM,BLACKHOLE,FEDERATED,ARCHIVE,MEMORY";
-- SET GLOBAL transaction_write_set_extraction=XXHASH64;
-- SET GLOBAL group_replication_group_name="aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
-- SET GLOBAL group_replication_start_on_boot=off;
-- SET GLOBAL group_replication_local_address= "mysqlDB:33061";
-- SET GLOBAL group_replication_group_seeds= "mysqlDB:33061,mysqlDB:33061,mysqlDB:33061";
-- SET GLOBAL group_replication_bootstrap_group=off;
-- SET GLOBAL group_replication_ip_whitelist = '127.0.0.1/8,172.0.0.1/8,192.168.0.0/16,10.0.0.1/8,10.18.89.49/22';


-- show variables like 'group%';

-- SELECT * FROM performance_schema.replication_group_members;
install PLUGIN group_replication SONAME 'group_replication.so';
install PLUGIN clone SONAME 'mysql_clone.so';
SET GLOBAL transaction_write_set_extraction=XXHASH64;
SET GLOBAL group_replication_group_name="aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";
SET GLOBAL group_replication_start_on_boot=off;
SET GLOBAL group_replication_local_address= "MAC_DISPATCH_SERVER_1:33061";
SET GLOBAL group_replication_group_seeds= "MAC_DISPATCH_SERVER_1:33061,MAC_DISPATCH_SERVER_2:33061";
SET GLOBAL group_replication_bootstrap_group=off;
SET GLOBAL group_replication_ip_whitelist = '127.0.0.1/8,172.0.0.1/8,192.168.0.0/16,10.0.0.1/8,10.18.89.49/22,MAC_DISPATCH_SERVER_1/24,MAC_DISPATCH_SERVER_2/24';
-- SET GLOBAL group_replication_unreachable_majority_timeout=5
grant CLONE_ADMIN on *.* to 'replication'@'%';
grant BACKUP_ADMIN  on *.* to 'replication'@'%';

-- show variables like 'group%';

SET GLOBAL group_replication_bootstrap_group=ON;

CHANGE MASTER TO MASTER_USER='replication', MASTER_PASSWORD='11111111' FOR CHANNEL 'group_replication_recovery';

-- START GROUP_REPLICATION USER='replication', PASSWORD='11111111';
START GROUP_REPLICATION;

SET GLOBAL group_replication_bootstrap_group=OFF;

-- SELECT * FROM performance_schema.replication_group_members;