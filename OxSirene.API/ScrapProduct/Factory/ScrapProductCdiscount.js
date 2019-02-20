function check(content) {
    let pattern_offer_list = "(<ul class=\\\"offersList jsOffersList\\\"[^>]+>)";
    let pattern_offer_list_uri = "(data-url=\\\")([^\\\"]+)";

    let regex_offer_list = new RegExp(pattern_offer_list);
    let regex_offer_list_uri = new RegExp(pattern_offer_list_uri);

    sellers = [];

    let matches = content.match(regex_offer_list);
    if (matches) {
        for (let match of matches) {
            let match2 = match.match(regex_offer_list_uri);
            if (match2) {
                sellers.push(match2[1]);
            }
        }
    }

    return sellers.length > 0;
}