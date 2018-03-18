interface JQuery {
    paging(number: number, options?: any): paging;
}

interface paging extends JQuery {
    setPage(page: number): void;
}