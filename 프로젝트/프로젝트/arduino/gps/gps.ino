#include <TinyGPS.h>
 
uint8_t _hour, _minute, _second, _year, _month, _day; // GPS로부터 시간값 읽기
 
#define GPSBAUD 9600
 
TinyGPS gps;
 
float latitude, longitude;
 
void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);

  Serial.println("START...");
  
  Serial1.begin(GPSBAUD);
  Serial1.setTimeout(10);
 
  gps = TinyGPS();
}
 
void loop() {
  // put your main code here, to run repeatedly:
    // GPS
    {
      // loop_gps();
      
      String temp = "";
      
      while(Serial1.available())     // While there is data on the RX pin...
      {
        char c = Serial1.read();    // load the data into a variable...
        
          if(gps.encode(c))      // if there is a new valid sentence...
          {
            getgps(gps);         // then grab the data.
          }   
        
      } 
 
      if(0 < temp.length())
      {
        
  
      }
      
    }
}
 
// The getgps function will get and print the values we want.
void getgps(TinyGPS &gps)
{
  // To get all of the data into varialbes that you can use in your code, 
  // all you need to do is define variables and query the object for the 
  // data. To see the complete list of functions see keywords.txt file in 
  // the TinyGPS and NewSoftSerial libs.
  
  // Define the variables that will be used
  // float latitude, longitude;
  // Then call this function
  gps.f_get_position(&latitude, &longitude);
  // You can now print variables latitude and longitude
  Serial.print("Lat/Long: "); 
  Serial.print(latitude,5); 
  Serial.print(", "); 
  Serial.println(longitude,5);
  
  // Same goes for date and time
  int year;
  byte month, day, hour, minute, second, hundredths;
  gps.crack_datetime(&year,&month,&day,&hour,&minute,&second,&hundredths);
  // Print data and time
  Serial.print("Date: "); Serial.print(month, DEC); Serial.print("/"); 
  Serial.print(day, DEC); Serial.print("/"); Serial.print(year);
  Serial.print("  Time: "); Serial.print(hour, DEC); Serial.print(":"); 
  Serial.print(minute, DEC); Serial.print(":"); Serial.print(second, DEC); 
  Serial.print("."); Serial.println(hundredths, DEC);
  //Since month, day, hour, minute, second, and hundr
 
  _year = year-2000;
  _month = month;
  _day = day;
  _hour = hour;
  _minute = minute;
  _second = second;
  
  // Here you can print the altitude and course values directly since 
  // there is only one value for the function
  Serial.print("Altitude (meters): "); Serial.println(gps.f_altitude());  
  // Same goes for course
  Serial.print("Course (degrees): "); Serial.println(gps.f_course()); 
  // And same goes for speed
  Serial.print("Speed(kmph): "); Serial.println(gps.f_speed_kmph());
  Serial.println();
  
  // Here you can print statistics on the sentences.
  unsigned long chars;
  unsigned short sentences, failed_checksum;
  gps.stats(&chars, &sentences, &failed_checksum);
  //Serial.print("Failed Checksums: ");Serial.print(failed_checksum);
  //Serial.println(); Serial.println();
  // delay(10000);
}
