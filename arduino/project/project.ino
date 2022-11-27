
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
#include "I2Cdev.h"
#include "MPU6050.h"
#include <EduIntro.h>
#include <TinyGPS.h>
 //gps 센서
uint8_t _hour, _minute, _second, _year, _month, _day; // GPS로부터 시간값 읽기
 
#define GPSBAUD 9600
 
TinyGPS gps;
 
float latitude, longitude;

//자이로 센서
MPU6050 accelgyro;

int16_t ax, ay, az;
int16_t gx, gy, gz;
//MQ-135센서
#define RLOAD 10.0
#define RZERO 76.63

#define PARA 116.6020682

#define PARB 2.769034857
///nrf24 통신
RF24 radio(7, 8); // SPI 버스에 nRF24L01 라디오를 설정하기 위해 CE, CSN를 선언.
const byte address[6] = "00001"; //주소값을 5가지 문자열로 변경할 수 있으며, 송신기와 수신기가 동일한 주소로 해야됨.
////온습도 센서
DHT11 dht11(D6);  // creating the object sensor on pin 'D7'

int C;   // temperature C readings are integers
float F; // temperature F readings are returned in float format
int H;   // humidity readings are integers

void setup() {
  #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
    Wire.begin();
  #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
    Fastwire::setup(400, true);
  #endif
  
  radio.begin();
  radio.openWritingPipe(address); //이전에 설정한 5글자 문자열인 데이터를 보낼 수신의 주소를 설정
  radio.setPALevel(RF24_PA_MAX); //전원공급에 관한 파워레벨을 설정합니다. 모듈 사이가 가까우면 최소로 설정합니다.
  //거리가 가까운 순으로 RF24_PA_MIN / RF24_PA_LOW / RF24_PA_HIGH / RF24_PA_MAX 등으로 설정할 수 있습니다.
  //높은 레벨(거리가 먼 경우)은 작동하는 동안 안정적인 전압을 가지도록 GND와 3.3V에 바이패스 커패시터 사용을 권장함. 
  radio.stopListening();  //모듈을 송신기로 설정
  

  Serial.begin(9600);

  //Serial.println("Initializing I2C devices...");
  accelgyro.initialize();

  //Serial.println("Testing device connections...");
  Serial.println(accelgyro.testConnection() ? "MPU6050 connection successful" : "MPU6050 connection failed");
  
  Serial1.begin(GPSBAUD);
  Serial1.setTimeout(10);
 
  gps = TinyGPS();
}
void loop() {
  delay(1);
 //자이로 센서 가져오기
 accelgyro.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);
   ///온습도 센서값 가져오기
   dht11.update();

  C = dht11.readCelsius();      // Reading the temperature in Celsius degrees and store in the C variable
  H = dht11.readHumidity();     // Reading the humidity index
//30000~-30000 을 100~-100으로 변환
  ax = map(ax, -30000, 30000, -100, 100);
  ay = map(ay, -30000, 30000, -100, 100);
  az = map(az, -30000, 30000, -100, 100);
  //MQ-135 센서 값 가져오기 
  int val = analogRead(A0);
  //센서값 변환{
  float Resistance;
  float PPM;
  val = (1023./(float)val) * 5. - 1.* RLOAD;
  Resistance = val;
  PPM = PARA * pow((Resistance/RZERO), -PARB);
  //Serial.println(PPM);
  String text1= "a"+String(PPM);
  //Serial.println(PPM);
  char text[text1.length()+2];
  text1.toCharArray(text, text1.length()+2);
  radio.write(&text, sizeof(text)); //MQ-135 메시지를 수신자에게 보냄
  //}
 // delay(100);
  //아두이노 자이로 센서값 보내기{
  char sum[12];
  sprintf(sum , "x%03dY%03dZ%03d", ax, ay, az);
  //Serial.println((String)sum);
  radio.write(&sum, sizeof(sum));//자이로 세서값 전송
  //}
  //delay(100);
  char temp[12];
  sprintf(temp , "C%03dH%03d", C, H);
  //Serial.println((String)temp);
  radio.write(&temp, sizeof(temp));//자이로 세서값 전송
  //gps센서{
  while(Serial1.available())     // While there is data on the RX pin...
  {
    char c = Serial1.read();    // load the data into a variable...
        
    if(gps.encode(c))      // if there is a new valid sentence...
    {
      getgps(gps);         // then grab the data.
    }    
  }
  //}
  //delay(1000);
}
void getgps(TinyGPS &gps)
{
  gps.f_get_position(&latitude, &longitude);
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
  
  //////////////////////////////////////////////////////////////////////////전송
  
  char temp[12];
  sprintf(temp , "S%03d", gps.f_altitude());
  Serial.println((String)temp);
  radio.write(&temp, sizeof(temp));//gps 속도 값 전송
  delay(100);
  /*
  char temp1[32];
  sprintf(temp1 , "D%3.6fL3.6f", latitude, longitude);
  Serial.println((String)temp1);
  radio.write(&temp1, sizeof(temp1));//gps 위도 경도 전송
*/
  String text1= "D"+String(latitude,6);
  char text[text1.length()+2];
  text1.toCharArray(text, text1.length()+2);
  radio.write(&text, sizeof(text)); //MQ-135 메시지를 수신자에게 보냄
  delay(100);
  
  String gs= "M"+String(longitude,6);
  char gc[gs.length()+2];
  gs.toCharArray(gc, gs.length()+2);
  radio.write(&gc, sizeof(gc)); //MQ-135 메시지를 수신자에게 보냄
  
  //delay(100);
  // Same goes for course///////////////////////////////////
  
  // Same goes for course///////////////////////////////////
  
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
