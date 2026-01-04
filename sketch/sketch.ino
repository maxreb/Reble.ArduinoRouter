/*
    This file is part of the Arduino_RouterBridge library.

    Copyright (c) 2025 Arduino SA

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/.
    
*/

#include <Arduino_RouterBridge.h>


bool set_led(byte numLed, bool state) {
    if (numLed < 0 || numLed > 1)
      return false;
    digitalWrite(LED_BUILTIN + (numLed * 3), state ? LOW : HIGH);
    return true;
}

int add(int a, int b) {
    return a + b;
}

String greet() {
    return String("Hello Friend");
}

void setup() {
    Monitor.begin();
    
    pinMode(LED_BUILTIN, OUTPUT); //RED LED 3
    pinMode(LED_BUILTIN + 3, OUTPUT);//RED LED 4
    digitalWrite(LED_BUILTIN, HIGH);
    digitalWrite(LED_BUILTIN + 3, HIGH);

    Bridge.begin();

    if (!Bridge.provide("set_led", set_led)) {
        Monitor.println("Error providing method: set_led");
    } else {
        Monitor.println("Registered method: set_led");
    }

    Bridge.provide("add", add);

    Bridge.provide_safe("greet", greet);

}

void loop() {
    float res;
    static int factor1;
    factor1 = (factor1 + 1) % 20;

    // Call with deferred response check
    RpcCall outcome = Bridge.call("multiply", (float)factor1, 7.0);
    Monitor.println("RPC called");
    if (outcome.result(res)) {
        Monitor.print("Result of the operation is: ");
        Monitor.println(res);
    } else {
        Monitor.println(outcome.getErrorCode());
        Monitor.println(outcome.getErrorMessage());
    }

    delay(1000);
}