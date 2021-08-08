xquery version "3.1" encoding "utf-8";

import module namespace functx="http://www.functx.com" at "http://www.xqueryfunctions.com/xq/functx-1.0.1-doc.xq";

element div {
    attribute class { "content" },
    for $doc in collection('../SpecE/?select=*.xml;recurse=yes')
    let $section := lower-case(tokenize(document-uri($doc), "[/]")[last()-1])
    let $name := $doc/root/func/title/text()
    let $comment := $doc/root/func/doc/cmn
    let $prototype := $doc/root/func/declare/vc/prottype/text()
    let $arguments := $doc/root/func/argument//item
    let $errors := $doc/root/func/errcode/item
    where $doc/root/func
    order by $section, $name
    return
        element div {
            attribute class { "overflow-hidden content-section" },
            attribute id { concat("content-", $section, "-", $name) },
            element h2 {
                attribute id { concat($section, "-", $name) },
                concat($section, "\", $name) 
            },
            element p { $comment },
            element br { },
            (::)
            element h4 { "PROTOTYPE" },
            element p {
                element code { $prototype }
            },
            element br {
            
            },
            (::)
            element h4 { "ARGUMENTS" },
            element table {
                element thead {
                    element tr {
                        element th { "Name" },
                        element th { "Direction" },
                        element th { "Description" }
                    }
                },
                element tbody {
                    for $argument in $arguments
                    return
                        element tr {
                            element td { $argument/name/text() },
                            element td { $argument/type/text() },
                            element td { $argument/content }
                        }
                }
            },
            element br { },
            (::)
            element h4 { "ERRORS" },
            element table {
                element thead {
                    element tr {
                        element th { "Code" },
                        element th { "Description" }
                    }
                },
                element tbody {
                    for $error in $errors
                    return
                        element tr {
                            element td { $error/name/text() },
                            element td { $error/content }
                        }
                }
            }
        }
}
