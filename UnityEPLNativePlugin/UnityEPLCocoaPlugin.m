//
//  UnityEPLCocoaPlugin.m
//  UnityEPLNativePlugin
//
//  Created by Henry Jonas Solberg on 7/31/17.
//

#import "UnityEPLCocoaPlugin.h"

NSMutableArray * KeyCodeQueue;
NSMutableArray * KeyTimestampQueue;
NSMutableArray * MouseButtonQueue;
NSMutableArray * MouseTimestampQueue;

NSObject * mouseHandler;
NSObject * keyboardHandler;

//returns the current uptime in milliseconds
//call this once in order to begin listening
//for everns and place nsevents on the c# stopwatch
double StartCocoaPlugin(void)
{
    KeyCodeQueue = [NSMutableArray new];
    KeyTimestampQueue = [NSMutableArray new];
    MouseButtonQueue = [NSMutableArray new];
    MouseTimestampQueue = [NSMutableArray new];
    
    mouseHandler =
    [NSEvent addLocalMonitorForEventsMatchingMask: (NSEventMaskLeftMouseUp |
                                                    NSEventMaskRightMouseUp |
                                                    NSEventMaskOtherMouseUp |
                                                    NSEventMaskLeftMouseDown |
                                                    NSEventMaskRightMouseDown |
                                                    NSEventMaskOtherMouseDown)
                                          handler: ^NSEvent* (NSEvent* handledEvent)
     {
         handleMouseEvent(handledEvent);
         return handledEvent;
     }
     ];

    keyboardHandler =
    [NSEvent addLocalMonitorForEventsMatchingMask: (NSEventMaskKeyUp |
                                                    NSEventMaskKeyDown)
                                          handler: ^NSEvent* (NSEvent* handledEvent)
     {
         handleKeyboardEvent(handledEvent);
         return handledEvent;
     }
     ];
    
    return [[NSProcessInfo processInfo] systemUptime];
}

void StopCocoaPlugin()
{
    [NSEvent removeMonitor:mouseHandler];
    [NSEvent removeMonitor:keyboardHandler];
}


int PopKeyKeycode(void)
{
    int keyCode = [[KeyCodeQueue objectAtIndex:0] intValue];
    [KeyCodeQueue removeObjectAtIndex:0];
    return keyCode;
}

double PopKeyTimestamp(void)
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

double PopMouseTimestamp(void)
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

void handleMouseEvent (NSEvent * handledEvent)
{
    int mouseButton = (int)[handledEvent buttonNumber];
    float time = [handledEvent timestamp];
    [MouseButtonQueue addObject: [NSNumber numberWithInt:mouseButton]];
    [MouseTimestampQueue addObject: [NSNumber numberWithFloat:time]];
    NSLog(@"%f", (double)[MouseButtonQueue count]);
}

void handleKeyboardEvent (NSEvent * handledEvent)
{
    int keyCode = [handledEvent keyCode];
    float time = [handledEvent timestamp];
    [KeyCodeQueue addObject: [NSNumber numberWithInt:keyCode]];
    [KeyTimestampQueue addObject: [NSNumber numberWithFloat:time]];
    NSLog(@"%f", (double)[MouseButtonQueue count]);
}
