// Type definitions for touchwipe

interface JQuery {
    touchwipe( settings?: touchwipeOptions): touchwipe;
}

interface touchwipe extends JQuery {
}

interface callbackFunction {
    (): void;
}

interface touchwipeOptions {
    min_move_x?: number;
    min_move_y?: number;
    wipeLeft?: callbackFunction;
    wipeRight?: callbackFunction;
    wipeUp?: callbackFunction;
    wipeDown?: callbackFunction;
    preventDefaultEvents?: boolean;
}