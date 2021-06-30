xquery version "3.1" encoding "utf-8";

import module namespace functx="http://www.functx.com" at "http://www.xqueryfunctions.com/xq/functx-1.0.1-doc.xq";

element table {
    element tr {
        element td { "name" },
        element td { "comment" },
        element td { "reference" },
        element td { "prototype" }
    },
    for $doc in collection('../SpecE/?select=*.xml;recurse=yes')
    let $name := $doc/root/func/title/text()
    let $prototype := functx:trim(normalize-space(string-join($doc/root/func/declare/vc/prottype/string(), "")))
    let $references := 
        for $reference in $doc/root/func/reference/item/name/text()
        return concat("[", $reference, "](#", $reference, ")")
    let $comment := 
        if (string-length(functx:trim($doc/root/func/doc/cmn[1]/text()[1])) > 0) then
            normalize-space($doc/root/func/doc/cmn[1]/text()[1])
        else
            normalize-space($doc/root/func/doc/cmn[1]/p[1]/text()[1])
    let $comment_full := $doc/root/func/doc
    where $doc/root/func
    return
        element tr {
            element td {
                element a { attribute name { $name } },
                $name
            },
            element td { 
                $comment 
            },
            element td {
                string-join($references, ", ")
            },
            element td {
                $prototype
            }
        }
}


