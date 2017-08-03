//
//  UnityEPLCocoaPlugin.m
//  UnityEPLNativePlugin
//
//  Created by Henry Jonas Solberg on 7/31/17.
//

#import "UnityEPLCocoaPlugin.h"

NSView* view = nil;

MouseInput * mouseInput = nil;

//returns the current uptime in milliseconds
//call this once in order to begin listening
//for everns and place nsevents on the c# stopwatch
double StartCocoaPlugin(void)
{
    KeyCodeQueue = [NSMutableArray new];
    KeyTimestampQueue = [NSMutableArray new];
    MouseButtonQueue = [NSMutableArray new];
    MouseTimestampQueue = [NSMutableArray new];
    
    [NSEvent addGlobalMonitorForEventsMatchingMask: (NSEventMaskLeftMouseUp |
                                                     NSEventMaskRightMouseUp |
                                                     NSEventMaskOtherMouseUp |
                                                     NSEventMaskLeftMouseDown |
                                                     NSEventMaskRightMouseDown |
                                                     NSEventMaskOtherMouseDown)
                                           handler: ^( NSEvent * handledEvent)
         {
             handleMouseEvent(handledEvent);
         }
     ];
    
    [NSEvent addGlobalMonitorForEventsMatchingMask: (NSEventMaskKeyUp |
                                                     NSEventMaskKeyDown)
                                           handler: ^( NSEvent * handledEvent)
         {
             handleKeyboardEvent(handledEvent);
         }
     ];
    
    
    NSApplication* app = [NSApplication sharedApplication];
    NSWindow* window = [app mainWindow];
    view = [window contentView];
    
    mouseInput = [MouseInput alloc];
    [view setNextResponder:mouseInput];

    
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
    NSLog(@"keyboard event");
    [KeyCodeQueue addObject: [NSNumber numberWithInt:keyCode]];
    [KeyTimestampQueue addObject: [NSNumber numberWithFloat:time]];
}

@implementation MouseInput

- (void) mouseDown:(NSEvent *)event
{
    handleMouseEvent(event);
}

@end
