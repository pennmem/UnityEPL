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
    [self handleInputEvent:theEvent];
}

- (void)keyDown:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)mouseUp:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)mouseDown:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)rightMouseUp:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)rightMouseDown:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)otherMouseUp:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)otherMouseDown:(NSEvent *)theEvent
{
    [self handleInputEvent:theEvent];
}

- (void)handleInputEvent:(NSEvent *)theEvent
{
    int keyCode = [theEvent keyCode];
    int mouseButton = (int)[theEvent buttonNumber];
    float time = [theEvent timestamp];
    if (mouseButton == 0)
    {
        [KeyCodeQueue addObject: [NSNumber numberWithInt:keyCode]];
        [KeyTimestampQueue addObject: [NSNumber numberWithFloat:time]];
    }
    else
    {
        [MouseButtonQueue addObject: [NSNumber numberWithInt:mouseButton]];
        [MouseTimestampQueue addObject: [NSNumber numberWithFloat:time]];
    }
}

@end
