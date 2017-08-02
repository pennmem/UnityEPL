//
//  UnityEPLCocoaPlugin.m
//  UnityEPLNativePlugin
//
//  Created by Henry Jonas Solberg on 7/31/17.
//

#import "UnityEPLCocoaPlugin.h"

//returns the current uptime in milliseconds
//call this once in order to begin listening
//for everns and place nsevents on the c# stopwatch
double StartCocoaPlugin(void)
{
    return [[NSProcessInfo processInfo] systemUptime] * 1000;
}


int PopKeyKeycode(void)
{
    int keyCode = [[KeyCodeQueue objectAtIndex:0] intValue];
    [KeyCodeQueue removeObjectAtIndex:0];
    return keyCode;
}

float PopKeyTimestamp(void)
{
    float keyTimestamp = [[KeyTimestampQueue objectAtIndex:0] floatValue];
    [KeyTimestampQueue removeObjectAtIndex:0];
    return keyTimestamp;
}

int CountKeyEvents(void)
{
    int keyCodeCount = (int)[KeyCodeQueue count];
    return keyCodeCount;
}

int PopMouseButton(void)
{
    int mouseButton = [[MouseButtonQueue objectAtIndex:0] intValue];
    [MouseButtonQueue removeObjectAtIndex:0];
    return mouseButton;
}

float PopMouseTimestamp(void)
{
    float mouseTimestamp = [[MouseTimestampQueue objectAtIndex:0] floatValue];
    [MouseTimestampQueue removeObjectAtIndex:0];
    return mouseTimestamp;
}

int CountMouseEvents(void)
{
    int mouseButtonCount = (int)[MouseButtonQueue count];
    return mouseButtonCount;
}




@implementation UnityEPLCocoaPlugin

- (void)keyUp:(NSEvent *)theEvent
{
    [self handleKeyEvent:theEvent];
}

- (void)keyDown:(NSEvent *)theEvent
{
    [self handleKeyEvent:theEvent];
}

- (void)handleKeyEvent:(NSEvent *)theEvent
{
    int keyCode = [theEvent keyCode];
    float time = [theEvent timestamp];
    [KeyCodeQueue addObject: [NSNumber numberWithInt:keyCode]];
    [KeyTimestampQueue addObject: [NSNumber numberWithFloat:time]];
}

@end
