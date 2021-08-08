xquery version "3.1" encoding "utf-8";

import module namespace functx="http://www.functx.com" at "http://www.xqueryfunctions.com/xq/functx-1.0.1-doc.xq";

element div {
    attribute class { "content-menu" },
    element ul {
        for $doc in collection('../SpecE/?select=*.xml;recurse=yes')
        let $section := lower-case(tokenize(document-uri($doc), "[/]")[last()-1])
        let $name := $doc/root/func/title/text()
        where $doc/root/func
        order by $section, $name
        return
            element li {
                attribute class { "scroll-to-link active" },
                attribute data-target { concat($section, "-", $name) },
                element a { 
                    (:attribute href { concat("#", $section, "-", $name) },:)
                    concat($section, "\", $name) 
                }
            }
    }
}
