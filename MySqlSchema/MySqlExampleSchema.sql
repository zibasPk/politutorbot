CREATE DATABASE  IF NOT EXISTS `politutor` /*!40100 DEFAULT CHARACTER SET utf8 */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `politutor`;
-- MySQL dump 10.13  Distrib 8.0.28, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: politutor
-- ------------------------------------------------------
-- Server version	8.0.28

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
-- Table structure for table `course`
--

DROP TABLE IF EXISTS `course`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `course` (
  `name` varchar(300) NOT NULL,
  `school` varchar(300) DEFAULT NULL,
  PRIMARY KEY (`name`),
  KEY `_idx` (`school`),
  CONSTRAINT `fk_course_school_name` FOREIGN KEY (`school`) REFERENCES `school` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `course`
--

LOCK TABLES `course` WRITE;
/*!40000 ALTER TABLE `course` DISABLE KEYS */;
INSERT INTO `course` VALUES ('Ingegneria Aerospaziale','3I'),('Ingegneria Biomedica','3I'),('Ingegneria Chimica','3I'),('Ingegneria dei Materiali e delle Nanotecnologie','3I'),('Ingegneria dell\'Automazione','3I'),('Ingegneria della Produzione Industriale','3I'),('Ingegneria Elettrica','3I'),('Ingegneria Elettronica','3I'),('Ingegneria Energetica','3I'),('Ingegneria Fisica','3I'),('Ingegneria Gestionale','3I'),('Ingegneria Informatica','3I'),('Ingegneria Matematica','3I'),('Ingegneria Meccanica','3I'),('Ingegneria Ambientale','ICAT'),('Ingegneria Civile','ICAT'),('Ingegneria Territoriale','ICAT');
/*!40000 ALTER TABLE `course` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `enabled_student`
--

DROP TABLE IF EXISTS `enabled_student`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `enabled_student` (
  `code` int NOT NULL,
  PRIMARY KEY (`code`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `enabled_student`
--

LOCK TABLES `enabled_student` WRITE;
/*!40000 ALTER TABLE `enabled_student` DISABLE KEYS */;
INSERT INTO `enabled_student` VALUES (111111),(222222),(938354);
/*!40000 ALTER TABLE `enabled_student` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `exam`
--

DROP TABLE IF EXISTS `exam`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `exam` (
  `name` varchar(300) NOT NULL,
  `year` varchar(2) NOT NULL,
  `course` varchar(300) NOT NULL,
  PRIMARY KEY (`name`,`course`),
  KEY `nm_idx` (`course`),
  CONSTRAINT `fk_exam_course_name` FOREIGN KEY (`course`) REFERENCES `course` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `exam`
--

LOCK TABLES `exam` WRITE;
/*!40000 ALTER TABLE `exam` DISABLE KEYS */;
INSERT INTO `exam` VALUES ('analisi 1','Y1','Ingegneria Civile'),('analisi 2','Y1','Ingegneria Civile'),('analisi 3','Y1','Ingegneria Civile'),('fondamenti di internet e reti','Y1','Ingegneria Civile'),('fondamenti di internet e reti 2','Y1','Ingegneria Civile'),('fondamenti di internet e reti 3','Y1','Ingegneria Civile');
/*!40000 ALTER TABLE `exam` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `school`
--

DROP TABLE IF EXISTS `school`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `school` (
  `name` varchar(300) NOT NULL,
  PRIMARY KEY (`name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `school`
--

LOCK TABLES `school` WRITE;
/*!40000 ALTER TABLE `school` DISABLE KEYS */;
INSERT INTO `school` VALUES ('3I'),('AUIC'),('Design'),('ICAT');
/*!40000 ALTER TABLE `school` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `telegram_user`
--

DROP TABLE IF EXISTS `telegram_user`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `telegram_user` (
  `userID` bigint NOT NULL,
  `student_number` int NOT NULL,
  `lock_timestamp` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  PRIMARY KEY (`userID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `telegram_user`
--

LOCK TABLES `telegram_user` WRITE;
/*!40000 ALTER TABLE `telegram_user` DISABLE KEYS */;
INSERT INTO `telegram_user` VALUES (123,123,'0000-00-00 00:00:00'),(321,321,'0000-00-00 00:00:00'),(107050697,111111,'0000-00-00 00:00:00'),(1089557436,111111,'2022-08-23 15:58:19');
/*!40000 ALTER TABLE `telegram_user` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tutor`
--

DROP TABLE IF EXISTS `tutor`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tutor` (
  `name` varchar(60) NOT NULL,
  `exam` varchar(300) NOT NULL,
  `course` varchar(300) NOT NULL,
  `school` varchar(300) NOT NULL,
  `ranking` int NOT NULL,
  `lock_timestamp` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',
  PRIMARY KEY (`name`),
  KEY `fk_tutor_exam_name_idx` (`exam`),
  KEY `fk_tutor_course_name_idx` (`course`),
  KEY `fk_tutor_school_name_idx` (`school`),
  CONSTRAINT `fk_tutor_course_name` FOREIGN KEY (`course`) REFERENCES `course` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tutor_exam_name` FOREIGN KEY (`exam`) REFERENCES `exam` (`name`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_tutor_school_name` FOREIGN KEY (`school`) REFERENCES `school` (`name`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb3;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tutor`
--

LOCK TABLES `tutor` WRITE;
/*!40000 ALTER TABLE `tutor` DISABLE KEYS */;
INSERT INTO `tutor` VALUES ('franceso marini','analisi 1','Ingegneria Informatica','3I',1,'2022-08-23 15:58:19'),('mario baresi','analisi 1','Ingegneria Fisica','3I',2,'0000-00-00 00:00:00'),('milo bontesi','analisi 1','Ingegneria Civile','ICAT',3,'0000-00-00 00:00:00');
/*!40000 ALTER TABLE `tutor` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'politutor'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2022-08-23 18:06:55
