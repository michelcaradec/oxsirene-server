function check(content) {
    let pattern_href_1 = "[\\\"'](\\/gp\\/help\\/seller\\/at-a-glance\\.html)[^\\\"']*[\\\"']";
    let pattern_href_2 = "[\\\"'](\\/gp\\/aag\\/main\\/ref=)[^\\\"']*[\\\"']";
    let pattern_seller = "(seller=)([^\\\"'&]*)";

    let regex_href_1 = new RegExp(pattern_href_1, "g");
    let regex_href_2 = new RegExp(pattern_href_2);
    let regex_seller = new RegExp(pattern_seller);

    sellers = [];

    for (let regex_href of [regex_href_1, regex_href_2]) {
        let matches = content.match(regex_href);
        if (!matches) {
            continue;
        }

        for (let match of matches) {
            let match2 = match.match(regex_seller);
            if (match2) {
                sellers.push(match2[2]);
            }
        }
    }

    return sellers.length > 0;
}