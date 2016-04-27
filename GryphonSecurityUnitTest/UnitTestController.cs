﻿using System;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using GryphonSecurity_v2_2.Domain.Entity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using GryphonSecurity_v2_2.Domain;
using System.Device.Location;

namespace GryphonSecurityTest
{
    [TestClass]
    public class UnitTestController
    {
        
        Controller control = Controller.Instance;
        User userTest = new User(1000, "firstnameTest", "lastnameTest");
        AlarmReport alarmReportTest;

        [TestMethod]
        public void TestMethodStartup()
        {
            Boolean notExpected = control.getStartup();

            //Starting the phone, this is the method we are testing.
            control.startUp();
            //The Boolean shouldnt be the same. before startup = true, after startup = false.
            Assert.AreNotSame(notExpected, control.getStartup());
        }

        [TestMethod]
        public void TestMethodCreateUser()
        {
            //creating a user, this is the method we are testing. It returns a boolean if it can save the user.


            control.createUser(userTest);
            String actualResult = control.getUser().toString();
            String expectedResult = userTest.toString();
            //Boolean actualResult = true;
            //Boolean expectedResult = false;
            Debug.WriteLine("user " + control.getUser().toString());
            Debug.WriteLine("expected Result " + expectedResult);
            //Testing if they are both the same, its testing if the created user is saved.
            Assert.AreEqual(expectedResult, actualResult);
        }
        [TestMethod]
        public void TestMethodCreateAlarmReport()
        {
            //Setting up alarm report object.
            setupAlarmReport("Test");
            //This is the method we are testing. We will need internet connection for it to be able to save it, if this is false it will save it locally (this will be tested later).
            var task = control.createAlarmReport(alarmReportTest);
            task.Wait();
            var response = task.Result;
            //We expect it to be true, since we have internet connection.
            Assert.AreEqual(true, response);
        }
        [TestMethod]
        public void TestMethodCreateTempAlarmReport()
        {
            //Setting up alarm report object.
            setupAlarmReport("Test");
            //This is the method we are testing. If we have no internet Connection it need to save it on local storage.
            Boolean actualResult = control.createTempAlarmReport(alarmReportTest);
            Assert.AreEqual(true, actualResult);
        }
        [TestMethod]
        public void TestMethodGetLocalStorageTempAlarmReports()
        {
            //This require to setup some alarm report objects and save it locally first.
            setupAlarmReport("Test1");
            control.createTempAlarmReport(alarmReportTest);
            String expectedResult = alarmReportTest.CustomerName;
            setupAlarmReport("Test2");
            control.createTempAlarmReport(alarmReportTest);
            //This is the method we are testing.
            List<AlarmReport> alarmReports = control.getLocalStorageTempAlarmReports();

            String actualResult = "";
            //we are looking for the alarm report with the name "Test1"
            foreach (AlarmReport alarmReport in alarmReports)
            {
                if (alarmReport.CustomerName.Equals("Test1"))
                    actualResult = alarmReport.CustomerName;
            }
            Assert.AreSame(expectedResult, actualResult);

        }
        //[TestMethod]
        //public void TestMethodReadDataFromNFCTag()
        //{
        //    String expectedResult = null;

        //    Boolean isConnected = control.checkNetworkConnection();
        //    String actualResult = control.readDataFromNFCTag(null, isConnected);

        //    Assert.AreSame(expectedResult, actualResult);
        //}
        [TestMethod]
        public void testGetDistance()
        {
            control.createUser(userTest);
            GeoCoordinate targetCoordinate = new GeoCoordinate(55.767944, 12.505161499999986);
            GeoCoordinate presentCoordinate = new GeoCoordinate(55.7705618401085, 12.5117938768867);
            Boolean expectedResult = true;
            var task = control.getDistance(presentCoordinate, targetCoordinate, "Lyngby st.");
            task.Wait();
            var actualResult = task.Result;
            Debug.WriteLine("getDistence: " + actualResult);
            Assert.AreEqual(expectedResult, actualResult);


        }

        [TestMethod]
        public void TestMethodOnLocationScan()
        {
            //its supposed to save on database
            String expectedResult = "Lyngby st.";
            var task = control.onLocationScan(expectedResult, true);
            task.Wait();
            var actualResult = task.Result;
            Assert.AreEqual(expectedResult, actualResult);
        }
        [TestMethod]
        public void TestMethodcalcPosition()
        {
            String expectedResult = "Alarmpanel stuen gammel bygning";
            GeoCoordinate presentCoordinate = new GeoCoordinate(55.767944, 12.505161499999986);
            var task = control.calcPosition("10c5bf4758f64559d4c2ca6adcd8fd08", presentCoordinate, true);
            task.Wait();
            var actualResult = task.Result;
            Assert.AreEqual(expectedResult, actualResult);

        }

        [TestMethod]
        public void TestCheckNetworkConnection()
        {
            Boolean expectedResult = true;
            Boolean actualResult = control.checkNetworkConnection();
            Assert.AreEqual(expectedResult, actualResult);
        }
        [TestMethod]
        public void TestSendPendingNFCs()
        {

            control.createUser(userTest);
            control.createLocalStorageNFCsTest(55.767944, 12.505161499999986, "10c5bf4758f64559d4c2ca6adcd8fd08");
            control.createLocalStorageNFCsTest(55.6713363, 12.566796599999975, "10c5bf4758f64559d4c2ca6adcd8fd08");
            int expectedResult = 2;
            Assert.AreEqual(expectedResult, control.getLocalStorageNFCs());
            var task = control.sendPendingNFCs();
            task.Wait();
            Assert.AreEqual(0, control.getLocalStorageNFCs());

        }
        [TestMethod]
        public void TestsendPendingAlarmReports()
        {
            control.createUser(userTest);
            setupAlarmReport("test");
            control.createLocalStorageAlarmReport(alarmReportTest);
            setupAlarmReport("test1");
            control.createLocalStorageAlarmReport(alarmReportTest);
            int expectedResult = 2;
            Assert.AreEqual(expectedResult, control.getLocalStorageAlarmReports());
            var task = control.sendPendingAlarmReports();
            task.Wait();
            Assert.AreEqual(0, control.getLocalStorageAlarmReports());

        }

        [TestMethod]
        public async Task TestMethodOnLocationScanNoConnection()
        {
            //its supposed to save on local storage
            String expectedResult = "10c5bf4758f64559d4c2ca6adcd8fd08";
            String actualResult = await control.onLocationScan("10c5bf4758f64559d4c2ca6adcd8fd08", false);
            Assert.AreEqual(expectedResult, actualResult);
        }



        private void setupAlarmReport(String name)
        {
            //random time needed for alarm report object.
            DateTime date = new DateTime(1337, 1, 1);
            String dateTest = date.ToString("yyyy:MM:dd");
            DateTime time = new DateTime(1337, 1, 1);
            String timeTest = time.ToString("H:mm:ss");
            DateTime cancelDuringEmergencyTime = new DateTime(1337, 1, 1);
            String cancelDuringEmergencyTimeTest = cancelDuringEmergencyTime.ToString("H:mm:ss");
            DateTime guardRadioedDate = new DateTime(1337, 1, 1);
            String guardRadioedDateTest = guardRadioedDate.ToString("yyyy:MM:dd");
            DateTime guardRadioedFrom = new DateTime(1337, 1, 1);
            String guardRadioedFromTest = guardRadioedFrom.ToString("H:mm:ss");
            DateTime guardRadioedTo = new DateTime(1337, 1, 1);
            String guardRadioedToTest = guardRadioedTo.ToString("H:mm:ss");
            DateTime arrivedAt = new DateTime(1337, 1, 1);
            String arrivedAtTest = arrivedAt.ToString("H:mm:ss");
            DateTime done = new DateTime(1337, 1, 1);
            String doneTest = done.ToString("H:mm:ss");
            //ends here.
            alarmReportTest = new AlarmReport(name, 1234567123, "streetAndHouseNumberTest", 1234, "cityTest", 12345678, dateTest,
                timeTest, "zoneTest", false, false, false, false, false, false, false, false, "000", false, cancelDuringEmergencyTimeTest, false, "", "remarkTest",
                "nameTest", "installerTest", "controlCenterTest", guardRadioedDateTest, guardRadioedFromTest, guardRadioedToTest,
                arrivedAtTest, doneTest, userTest.Id);
        }


    }
}
