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

NSView * view = nil;
InputResponder * inputResponder =  nil;

//returns the current uptime in milliseconds
//call this once in order to begin listening
//for everns and place nsevents on the c# stopwatch
double StartCocoaPlugin(void)
{
    KeyCodeQueue = [NSMutableArray new];
    KeyTimestampQueue = [NSMutableArray new];
    MouseButtonQueue = [NSMutableArray new];
    MouseTimestampQueue = [NSMutableArray new];
    
    NSApplication* app = [NSApplication sharedApplication];
    NSWindow * window = [app mainWindow];
    view = [window contentView];
    
    inputResponder = [InputResponder alloc];
    [window makeFirstResponder:view];
    [view setNextResponder:inputResponder];
    
    return [[NSProcessInfo processInfo] systemUptime] * 1000;
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

@implementation InputResponder

- (void) forwardInvocation:(NSInvocation *)anInvocation
{
    [anInvocation invokeWithTarget:_nextResponder];
}

- (BOOL) acceptsFirstResponder
{
    return YES;
}

- (BOOL) resignFirstResponder
{
    return NO;
}

- (void) mouseDown:(NSEvent *)event
{
    handleMouseEvent(event);
    [self.nextResponder mouseDown:event];
}

- (void) mouseUp:(NSEvent *)event
{
    handleMouseEvent(event);
    [self.nextResponder mouseUp:event];
}

- (void) rightMouseDown:(NSEvent *)event
{
    handleMouseEvent(event);
    [self.nextResponder rightMouseDown:event];
}

- (void) rightMouseUp:(NSEvent *)event
{
    handleMouseEvent(event);
    [self.nextResponder rightMouseUp:event];
}

- (void) otherMouseDown:(NSEvent *)event
{
    handleMouseEvent(event);
    [self.nextResponder otherMouseDown:event];
}

- (void) otherMouseUp:(NSEvent *)event
{
    handleMouseEvent(event);
    [self.nextResponder otherMouseUp:event];
}

- (void) keyDown:(NSEvent *)event
{
    handleKeyboardEvent(event);
    [self.nextResponder keyDown:event];
}

- (void) keyUp:(NSEvent *)event
{
    handleKeyboardEvent(event);
    [self.nextResponder keyUp:event];
}


@end
