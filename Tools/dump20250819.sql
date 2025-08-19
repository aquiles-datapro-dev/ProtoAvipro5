CREATE DATABASE  IF NOT EXISTS `avipro5` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `avipro5`;
-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: avipro5
-- ------------------------------------------------------
-- Server version	8.0.43

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `employees`
--

DROP TABLE IF EXISTS `employees`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `last_name` varchar(30) DEFAULT NULL,
  `first_name` varchar(30) DEFAULT NULL,
  `middle_initial` varchar(3) DEFAULT NULL,
  `address` text,
  `city` varchar(50) DEFAULT NULL,
  `zip_code` varchar(15) DEFAULT NULL,
  `home_phone` varchar(25) DEFAULT NULL,
  `beeper` varchar(25) DEFAULT NULL,
  `cellular` varchar(50) DEFAULT NULL,
  `emergency_contact` varchar(20) DEFAULT NULL,
  `emergency_phone` varchar(25) DEFAULT NULL,
  `emergency_relation` varchar(25) DEFAULT NULL,
  `hire_date` datetime DEFAULT NULL,
  `last_work_date` datetime DEFAULT NULL,
  `date_birth` datetime DEFAULT NULL,
  `title` varchar(40) DEFAULT NULL,
  `wages` decimal(19,4) DEFAULT NULL,
  `comments` longtext,
  `ss` varchar(15) DEFAULT NULL,
  `dep` int DEFAULT NULL,
  `shop` varchar(20) DEFAULT NULL,
  `m_s` varchar(1) DEFAULT NULL,
  `active` tinyint(1) DEFAULT NULL,
  `tech` tinyint(1) DEFAULT NULL,
  `insp` tinyint(1) DEFAULT NULL,
  `emp_ini` varchar(5) DEFAULT NULL,
  `username` varchar(100) DEFAULT NULL,
  `password` text,
  `emp_mail` varchar(50) DEFAULT NULL,
  `office_personal` tinyint(1) DEFAULT NULL,
  `service` tinyint(1) DEFAULT NULL,
  `department` varchar(50) DEFAULT NULL,
  `total_vac` int DEFAULT NULL,
  `used_vac` int DEFAULT NULL,
  `balance_vac` int DEFAULT NULL,
  `sing_off_title` varchar(50) DEFAULT NULL,
  `ri` tinyint(1) DEFAULT NULL,
  `ip` tinyint(1) DEFAULT NULL,
  `rts` tinyint(1) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `modified_at` datetime DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  `state_id` bigint NOT NULL,
  `role_id` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_tbl_employees_tbl_state` (`state_id`),
  KEY `fk_tbl_employees_tbl_roles` (`role_id`),
  CONSTRAINT `fk_tbl_employees_tbl_roles` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tbl_employees_tbl_state` FOREIGN KEY (`state_id`) REFERENCES `state` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employees`
--

LOCK TABLES `employees` WRITE;
/*!40000 ALTER TABLE `employees` DISABLE KEYS */;
INSERT INTO `employees` VALUES (1,'Perez Miranda','Aquiles','APM',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,1,NULL,NULL,NULL,'aquiles','$2a$12$DPJEgz.PxoHsyM96UTcjA.t55KUr.vQPawhHoBoLie2l4bTBE73jq',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,1,1);
/*!40000 ALTER TABLE `employees` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `employees_functionalities`
--

DROP TABLE IF EXISTS `employees_functionalities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `employees_functionalities` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `employee_id` bigint NOT NULL,
  `functionality_id` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_employees_functionalities_employees` (`employee_id`),
  KEY `fk_employees_functionalities_functionalities` (`functionality_id`),
  CONSTRAINT `fk_employees_functionalities_employees` FOREIGN KEY (`employee_id`) REFERENCES `employees` (`id`),
  CONSTRAINT `fk_employees_functionalities_functionalities` FOREIGN KEY (`functionality_id`) REFERENCES `functionalities` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `employees_functionalities`
--

LOCK TABLES `employees_functionalities` WRITE;
/*!40000 ALTER TABLE `employees_functionalities` DISABLE KEYS */;
/*!40000 ALTER TABLE `employees_functionalities` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `functionalities`
--

DROP TABLE IF EXISTS `functionalities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `functionalities` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `module_id` bigint NOT NULL,
  `name` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_functionalities_module` (`module_id`),
  CONSTRAINT `fk_functionalities_module` FOREIGN KEY (`module_id`) REFERENCES `module` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `functionalities`
--

LOCK TABLES `functionalities` WRITE;
/*!40000 ALTER TABLE `functionalities` DISABLE KEYS */;
/*!40000 ALTER TABLE `functionalities` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `module`
--

DROP TABLE IF EXISTS `module`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `module` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(100) DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `module`
--

LOCK TABLES `module` WRITE;
/*!40000 ALTER TABLE `module` DISABLE KEYS */;
/*!40000 ALTER TABLE `module` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(100) DEFAULT NULL,
  `description` text,
  `parent_role_id` bigint DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `modified_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_tbl_roles_tbl_roles` (`parent_role_id`),
  CONSTRAINT `fk_tbl_roles_tbl_roles` FOREIGN KEY (`parent_role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'admin','admin',NULL,'2025-08-19 00:00:00','2025-08-19 00:00:00');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles_functionalities`
--

DROP TABLE IF EXISTS `roles_functionalities`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles_functionalities` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `role_id` bigint NOT NULL,
  `functionality_id` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_roles_functionalities_functionalities` (`functionality_id`),
  KEY `fk_roles_functionalities_roles` (`role_id`),
  CONSTRAINT `fk_roles_functionalities_functionalities` FOREIGN KEY (`functionality_id`) REFERENCES `functionalities` (`id`),
  CONSTRAINT `fk_roles_functionalities_roles` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles_functionalities`
--

LOCK TABLES `roles_functionalities` WRITE;
/*!40000 ALTER TABLE `roles_functionalities` DISABLE KEYS */;
/*!40000 ALTER TABLE `roles_functionalities` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `state`
--

DROP TABLE IF EXISTS `state`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `state` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `abbr` varchar(50) DEFAULT NULL,
  `name` varchar(100) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `modified_at` datetime DEFAULT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `state`
--

LOCK TABLES `state` WRITE;
/*!40000 ALTER TABLE `state` DISABLE KEYS */;
INSERT INTO `state` VALUES (1,'fl','florida',NULL,NULL);
/*!40000 ALTER TABLE `state` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vendors`
--

DROP TABLE IF EXISTS `vendors`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vendors` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `vendor_name` varchar(50) DEFAULT NULL,
  `address` text,
  `city` varchar(25) DEFAULT NULL,
  `zip_code` varchar(15) DEFAULT NULL,
  `phone` varchar(25) DEFAULT NULL,
  `fax` varchar(25) DEFAULT NULL,
  `contact` varchar(100) DEFAULT NULL,
  `payment_method` varchar(50) DEFAULT NULL,
  `terms` int DEFAULT NULL,
  `bal_due` decimal(19,4) DEFAULT NULL,
  `vendor_tax` float DEFAULT NULL,
  `internet_address` varchar(50) DEFAULT NULL,
  `acct_number` varchar(50) DEFAULT NULL,
  `fast_type` int DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  `contact_second` varchar(100) DEFAULT NULL,
  `country` varchar(50) DEFAULT NULL,
  `bill_add_1` varchar(50) DEFAULT NULL,
  `bill_add_2` varchar(50) DEFAULT NULL,
  `bill_city` varchar(50) DEFAULT NULL,
  `bill_state` varchar(2) DEFAULT NULL,
  `bill_zip` varchar(50) DEFAULT NULL,
  `bill_country` varchar(50) DEFAULT NULL,
  `audit_vendor` tinyint(1) DEFAULT NULL,
  `last_audit_date` datetime DEFAULT NULL,
  `blacklist` tinyint(1) DEFAULT NULL,
  `date_audit_sent` datetime DEFAULT NULL,
  `audit_notes` text,
  `easa_date` datetime DEFAULT NULL,
  `drug_pro_date` datetime DEFAULT NULL,
  `iso_date` datetime DEFAULT NULL,
  `nap_cap_date` datetime DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `modified_at` datetime DEFAULT NULL,
  `state_id` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_tbl_vendors_tbl_state` (`state_id`),
  CONSTRAINT `fk_tbl_vendors_tbl_state` FOREIGN KEY (`state_id`) REFERENCES `state` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vendors`
--

LOCK TABLES `vendors` WRITE;
/*!40000 ALTER TABLE `vendors` DISABLE KEYS */;
/*!40000 ALTER TABLE `vendors` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `vendors_contact`
--

DROP TABLE IF EXISTS `vendors_contact`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `vendors_contact` (
  `id` bigint NOT NULL AUTO_INCREMENT,
  `name` varchar(50) DEFAULT NULL,
  `title` int DEFAULT NULL,
  `email` varchar(50) DEFAULT NULL,
  `phone` varchar(50) DEFAULT NULL,
  `fax` varchar(50) DEFAULT NULL,
  `created_at` datetime DEFAULT NULL,
  `modified_at` datetime DEFAULT NULL,
  `vendor_id` bigint NOT NULL,
  PRIMARY KEY (`id`),
  KEY `fk_tbl_vendors_contact_tbl_vendors` (`vendor_id`),
  CONSTRAINT `fk_tbl_vendors_contact_tbl_vendors` FOREIGN KEY (`vendor_id`) REFERENCES `vendors` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `vendors_contact`
--

LOCK TABLES `vendors_contact` WRITE;
/*!40000 ALTER TABLE `vendors_contact` DISABLE KEYS */;
/*!40000 ALTER TABLE `vendors_contact` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'avipro5'
--

--
-- Dumping routines for database 'avipro5'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-08-19 16:12:23
