// Type definitions for jqcloud

///<reference path="../jquery/jquery.d.ts" />

interface JQuery {

    jQCloud(data: jQCloudData[], options?: jQCloudOptions): jQCloud;
}

interface jQCloud extends JQuery {
}

interface afterWordRender {
    // I'm not sure what the type of the first item is
    (item: any, wordSpan: JQuery): void;
}

interface afterCloudRender {
    // I'm not sure what the type of the first item is
    (item: any, jQCloud: jQCloud): void;
}

interface jQCloudData {
    text: string;
    weight: number;
    link?: string;
    html?: any;
    afterWordRender?: afterWordRender;
}

interface point {
    x: number;
    y: number;
}

interface jQCloudOptions {
    width?: number;
    height?: number;
    center?: point;
    encodeURI?: boolean;
    afterCloudRender?: afterCloudRender;
    delayedMode?: boolean;
    shape?: string; // "elliptic" (default) or "rectangular"
    removeOverflowing?: boolean;
}
