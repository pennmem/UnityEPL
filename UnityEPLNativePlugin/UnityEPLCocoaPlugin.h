//
//  UnityEPLCocoaPlugin.h
//  UnityEPLNativePlugin
//
//  Created by Henry Jonas Solberg on 7/31/17.
//

#import <Foundation/Foundation.h>
#import <Cocoa/Cocoa.h>

double StartCocoaPlugin(void);

int PopKeyKeycode(void);
double PopKeyTimestamp(void);
int CountKeyEvents(void);
int PopMouseButton(void);
double PopMouseTimestamp(void);
int CountMouseEvents(void);
void handleMouseEvent(NSEvent * theEvent);
void handleKeyboardEvent(NSEvent * theEvent);

@interface InputResponder : NSResponder {}

@end
