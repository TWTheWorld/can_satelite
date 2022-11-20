#include "I2Cdev.h"
#include "MPU6050.h"

MPU6050 accelgyro;

int16_t ax, ay, az;
int16_t gx, gy, gz;

void setup() {
    // join I2C bus (I2Cdev library doesn't do this automatically)
    #if I2CDEV_IMPLEMENTATION == I2CDEV_ARDUINO_WIRE
        Wire.begin();
    #elif I2CDEV_IMPLEMENTATION == I2CDEV_BUILTIN_FASTWIRE
        Fastwire::setup(400, true);
    #endif

    Serial.begin(9600);

    Serial.println("Initializing I2C devices...");
    accelgyro.initialize();

    Serial.println("Testing device connections...");
    Serial.println(accelgyro.testConnection() ? "MPU6050 connection successful" : "MPU6050 connection failed");
}

void loop() {
   accelgyro.getMotion6(&ax, &ay, &az, &gx, &gy, &gz);
   
   ax = map(ax, -30000, 30000, -100, 100);
   ay = map(ay, -30000, 30000, -100, 100);
   az = map(az, -30000, 30000, -100, 100); 
   //Serial.println("x %d y %d z %d",ax,ay,az);
   char sum[20];
//int num1 = 12345;
//char c = 'A';
//char str[] = "Hello";
sprintf(sum , "x%03dY%03dZ%03d", ax, ay, az);

Serial.println((String)sum);
   //Serial.println("y"+(String)ay);
   //Serial.println("z"+(String)az);
   delay(1000);
   //Serial.println(gz);
}
