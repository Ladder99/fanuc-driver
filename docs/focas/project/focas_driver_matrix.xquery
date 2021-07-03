xquery version "3.1" encoding "utf-8";

import module namespace functx="http://www.functx.com" at "http://www.xqueryfunctions.com/xq/functx-1.0.1-doc.xq";

(: https://tableconvert.com/ :)

element table {
    element tr {
        element td { "section" },
        element td { "name" },
        element td { "comment" },
        element td { "prototype" },
        element td { "platform" },
        element td { "veneer" }
    },
    for $doc in collection('../SpecE/?select=*.xml;recurse=yes')
    let $section := lower-case(tokenize(document-uri($doc), "[/]")[last()-1])
    let $name := $doc/root/func/title/text()
    let $prototype := concat("`", functx:trim(normalize-space(string-join($doc/root/func/declare/vc/prottype/string(), ""))), "`")
    let $comment := 
        if (string-length(functx:trim($doc/root/func/doc/cmn[1]/text()[1])) > 0) then
            normalize-space($doc/root/func/doc/cmn[1]/text()[1])
        else
            normalize-space($doc/root/func/doc/cmn[1]/p[1]/text()[1])
    where $doc/root/func
    order by $section, $name
    return
        element tr {
            element td { $section },
            element td { $name },
            element td { $comment },
            element td { $prototype },
            element td { "-" },
            element td { "-" }
        }
}


